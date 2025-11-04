using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class car : MonoBehaviour
{
    public Rigidbody rigid;
    public WheelCollider wheel1, wheel2, wheel3, wheel4;
    public float drivespeed, steerspeed;
    [Header("Brakes")] public float stopBrakeTorque =5000f;

    // Auto-conducción al iniciar
    [Header("Auto Drive")]
    public bool autoDrive = false;
    [Range(0f,1f)] public float autoDriveInput =1f; // acelerador constante hacia adelante

    private Vector2 moveInput;
    private CarControls controls;

    void Awake()
    {
        controls = new CarControls();
        controls.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    // Alinea el coche para mirar al eje X (+X por defecto)
    public void AlignToXAxis(bool positive = true)
    {
        var forward = positive ? Vector3.right : Vector3.left;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        // Reinicia velocidades para evitar desalineaciones por inercia
        if (rigid != null)
        {
            rigid.linearVelocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
        // Sin dirección al iniciar
        moveInput = new Vector2(0f, moveInput.y);
    }

    // Llamado por UIManager al pulsar Start
    public void StartAutoDrive()
    {
        autoDrive = true;
        // Opcional: anular input del usuario
        moveInput = new Vector2(0f, autoDriveInput);
        // Quita frenos si quedaron aplicados
        ApplyBrake(0f);
    }

    public void StopAutoDrive()
    {
        autoDrive = false;
        moveInput = Vector2.zero;
    }

    // Frena y detiene el auto inmediatamente
    public void StopImmediately()
    {
        autoDrive = false;
        moveInput = Vector2.zero;
        if (wheel1) { wheel1.motorTorque =0f; wheel1.steerAngle =0f; wheel1.brakeTorque = stopBrakeTorque; }
        if (wheel2) { wheel2.motorTorque =0f; wheel2.steerAngle =0f; wheel2.brakeTorque = stopBrakeTorque; }
        if (wheel3) { wheel3.motorTorque =0f; wheel3.brakeTorque = stopBrakeTorque; }
        if (wheel4) { wheel4.motorTorque =0f; wheel4.brakeTorque = stopBrakeTorque; }
        if (rigid != null) { rigid.linearVelocity = Vector3.zero; rigid.angularVelocity = Vector3.zero; }
    }

    private void ApplyBrake(float torque)
    {
        if (wheel1) wheel1.brakeTorque = torque;
        if (wheel2) wheel2.brakeTorque = torque;
        if (wheel3) wheel3.brakeTorque = torque;
        if (wheel4) wheel4.brakeTorque = torque;
    }

    // Devuelve +1 si el eje X local del WheelCollider apunta al lado izquierdo del coche (orientación correcta para avanzar con torque positivo)
    private float MotorSign(WheelCollider wc)
    {
        if (wc == null) return 1f;
        float d = Vector3.Dot(wc.transform.right, -transform.right); // X del wheel vs izquierda del coche
        return d >=0f ?1f : -1f;
    }

    // Devuelve +1 si el eje Z local del WheelCollider apunta hacia el frente del coche (para mantener el signo del ángulo de giro)
    private float SteerSign(WheelCollider wc)
    {
        if (wc == null) return 1f;
        float d = Vector3.Dot(wc.transform.forward, transform.forward);
        return d >=0f ?1f : -1f;
    }

    void FixedUpdate()
    {
        // Elegir input de aceleración: auto o jugador
        float accel = autoDrive ? autoDriveInput : moveInput.y;
        float motor = accel * drivespeed;

        // Aplica torque con el signo correcto según la orientación real de cada WheelCollider
        if (wheel1) wheel1.motorTorque = motor * MotorSign(wheel1);
        if (wheel2) wheel2.motorTorque = motor * MotorSign(wheel2);
        if (wheel3) wheel3.motorTorque = motor * MotorSign(wheel3);
        if (wheel4) wheel4.motorTorque = motor * MotorSign(wheel4);

        // Dirección: recto si es auto-drive. Ajusta signo si algún WheelCollider está invertido.
        float steerInput = autoDrive ?0f : moveInput.x;
        float steer = steerspeed * steerInput;
        if (wheel1) wheel1.steerAngle = steer * SteerSign(wheel1);
        if (wheel2) wheel2.steerAngle = steer * SteerSign(wheel2);
    }
}
