using UnityEngine;

public class Bar : MonoBehaviour
{
    public Vector2 StartPosition;
    public GameObject BarObject;
    public float maxLength = 1f;
    public BoxCollider bCollider;
    public HingeJoint startJoint;
    public HingeJoint endJoint;

    float StartJointCurrentLoad = 0f;
    float EndJointCurrentLoad = 0f;
    MaterialPropertyBlock propBlock;

    float maxForceMagnitude = 0f;
    [Header("Joint Break Settings (fallback)")]
    public float breakForce = 300f;
    public float breakTorque = Mathf.Infinity;

    // Auto-calibración/medición
    public bool autoCalibrateOnStart = false;
    [Range(0.1f, 5f)] public float calibrationDuration = 1.0f;
    [Range(0.1f, 1f)] public float calibrationFactor = 0.75f; //75% del pico
    float calibrationTimer = 0f;
    bool calibrating = false;
    bool applyCalibration = false; // si true, aplica breakForce al finalizar; si false, solo reporta

    // Reducir el largo del collider para evitar solapamientos en los extremos (en unidades de mundo por cada lado)
    public float colliderEndMargin = 0.14f;

    // Tipo de barra (carretera vs soporte de madera)
    public enum BarKind { Road, WoodSupport }
    [Header("Bar Type")]
    public BarKind kind = BarKind.Road;

    [Header("Wood Support Damping (3D Hinge)")]
    public bool woodUseSpring = true;
    public float woodSpring = 0f; // rigidez angular (puede quedar en0 si solo deseas amortiguar)
    public float woodDamper = 15f; // amortiguación angular para reducir el balanceo
    public float woodLinearDrag = 1.0f; // drag lineal extra para el RB de la barra
    public float woodAngularDrag = 2.0f; // drag angular extra para el RB de la barra

    [Header("Kind Default BreakForce")]
    public float roadDefaultBreakForce = 260f;
    public float woodDefaultBreakForce = 60f;

    [Header("Kind BreakForce Guards/Multipliers")]
    public float roadMinBreakForce = 200f;
    public float woodMinBreakForce = 400f;
    public float woodStrengthMultiplier = 2.0f; // multiplica el recomendado cuando se aplique a madera

    public void UpdateCreatingBar(Vector2 ToPosition)
    {
        transform.position = (ToPosition + StartPosition) / 2;

        Vector2 dir = ToPosition - StartPosition;
        float angle = Vector2.SignedAngle(Vector2.right, dir);
        transform.rotation = Quaternion.Euler(0, 0, angle);

        float length = dir.magnitude;
        // Escala visual completa
        if (BarObject != null)
        {
            BarObject.transform.localScale = new Vector3(length, BarObject.transform.localScale.y, BarObject.transform.localScale.z);
        }

        // Ajuste del BoxCollider: más corto para no chocar con puntos/baras adyacentes
        if (bCollider != null)
        {
            float lossyX = Mathf.Abs(bCollider.transform.lossyScale.x);
            // Largo del collider en mundo, recortado en ambos extremos
            float colliderWorldLength = Mathf.Max(0f, length - 2f * Mathf.Max(0f, colliderEndMargin));
            if (lossyX > 0.0001f)
            {
                float localX = colliderWorldLength / lossyX;
                bCollider.size = new Vector3(localX, bCollider.size.y, bCollider.size.z);
                bCollider.center = new Vector3(0f, bCollider.center.y, bCollider.center.z); // centrado
            }
        }
    }

    public void UpdateMaterial()
    {
        if (BarObject == null) return;

        // Cargas normalizadas por breakForce del propio joint para visualización
        if (startJoint != null && startJoint.breakForce > 0f)
        {
            float f = startJoint.currentForce.magnitude;
            StartJointCurrentLoad = f / startJoint.breakForce;
            if (f > maxForceMagnitude) maxForceMagnitude = f;
        }

        if (endJoint != null && endJoint.breakForce > 0f)
        {
            float f = endJoint.currentForce.magnitude;
            EndJointCurrentLoad = f / endJoint.breakForce;
            if (f > maxForceMagnitude) maxForceMagnitude = f;
        }

        float maxLoad = Mathf.Max(StartJointCurrentLoad, EndJointCurrentLoad);

        if (propBlock == null) propBlock = new MaterialPropertyBlock();

        // Adaptación: usar Renderer(s) del GameObject para aplicar MaterialPropertyBlock
        var renderers = BarObject.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null) continue;
            r.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_Load", maxLoad / Mathf.Max(0.0001f, maxLength));
            r.SetPropertyBlock(propBlock);
        }
    }

    private void Update()
    {
        if (Time.timeScale == 1)
        {
            UpdateMaterial();

            // Ventana de calibración (solo cuando el juego está corriendo y no pausado)
            if (calibrating && !GameManager.IsPaused)
            {
                calibrationTimer += Time.deltaTime;
                if (calibrationTimer >= calibrationDuration)
                {
                    float recommended = Mathf.Max(1f, maxForceMagnitude * calibrationFactor);
                    if (applyCalibration)
                    {
                        ApplyRecommendedBreakForce(recommended);
                    }
                    Debug.Log($"Medición fuerzas [{name}]: pico={maxForceMagnitude:F2} => recomendado breakForce={recommended:F2} (factor {calibrationFactor})");
                    calibrating = false;
                }
            }
        }
    }

    public void StartForceMeasurement(float duration, float factor, bool apply)
    {
        maxForceMagnitude = 0f;
        calibrationTimer = 0f;
        calibrationDuration = duration;
        calibrationFactor = factor;
        applyCalibration = apply;
        calibrating = true;
    }

    private void ApplyKindTuning()
    {
        var rb = GetComponent<Rigidbody>();
        if (kind == BarKind.WoodSupport)
        {
            // Amortiguación para reducir balanceo
            if (startJoint != null)
            {
                var sp = startJoint.spring;
                sp.spring = woodSpring;
                sp.damper = woodDamper;
                sp.targetPosition = 0f;
                startJoint.spring = sp;
                startJoint.useSpring = woodUseSpring;
            }
            if (endJoint != null)
            {
                var sp = endJoint.spring;
                sp.spring = woodSpring;
                sp.damper = woodDamper;
                sp.targetPosition = 0f;
                endJoint.spring = sp;
                endJoint.useSpring = woodUseSpring;
            }
            if (rb != null)
            {
                //3D Rigidbody
                rb.linearDamping = Mathf.Max(rb.linearDamping, woodLinearDrag);
                rb.angularDamping = Mathf.Max(rb.angularDamping, woodAngularDrag);
            }
        }
        else // Road
        {
            if (startJoint != null) startJoint.useSpring = false;
            if (endJoint != null) endJoint.useSpring = false;
        }
    }

    private void ApplyKindBreakForces()
    {
        float f = breakForce; // fallback
        switch (kind)
        {
            case BarKind.Road: f = roadDefaultBreakForce; break;
            case BarKind.WoodSupport: f = woodDefaultBreakForce; break;
        }
        ApplyBreakForce(f, breakTorque);
    }

    // Aplica una recomendación respetando mínimos y multiplicadores por tipo
    private void ApplyRecommendedBreakForce(float recommended)
    {
        float applied = recommended;
        if (kind == BarKind.Road)
        {
            applied = Mathf.Max(recommended, roadMinBreakForce);
        }
        else // WoodSupport
        {
            applied = Mathf.Max(recommended * woodStrengthMultiplier, woodMinBreakForce);
        }
        ApplyBreakForce(applied, breakTorque);
        Debug.Log($"[{name}] aplicado breakForce={applied:F2} (tipo {kind})");
    }

    public void ApplyBreakForce(float force, float torque)
    {
        if (startJoint != null)
        {
            startJoint.breakForce = force;
            startJoint.breakTorque = torque;
        }
        if (endJoint != null)
        {
            endJoint.breakForce = force;
            endJoint.breakTorque = torque;
        }
    }

    private void Awake()
    {
        // Ajustes específicos según tipo de barra
        ApplyKindTuning();
        ApplyKindBreakForces();
    }

    private void Start()
    {
        // Arranca la calibración si está habilitada
        if (Application.isPlaying && autoCalibrateOnStart)
        {
            StartForceMeasurement(calibrationDuration, calibrationFactor, true);
        }
    }

    [ContextMenu("Calibrate BreakForce From Peak (Set Now)")]
    private void CalibrateBreakForceFromPeak()
    {
        float recommended = Mathf.Max(1f, maxForceMagnitude * calibrationFactor);
        ApplyRecommendedBreakForce(recommended);
        Debug.Log($"Calibración manual [{name}]: pico={maxForceMagnitude:F2} => breakForce recomendado={recommended:F2} (factor {calibrationFactor})");
    }
}

