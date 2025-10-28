using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class car : MonoBehaviour
{
    public Rigidbody rigid;
    public WheelCollider wheel1, wheel2, wheel3, wheel4;
    public float drivespeed, steerspeed;

    // Auto-conducción al iniciar
    [Header("Auto Drive")]
    public bool autoDrive = false;
    [Range(0f, 1f)] public float autoDriveInput = 1f; // acelerador constante hacia adelante

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

    // Llamado por UIManager al pulsar Start
    public void StartAutoDrive()
    {
        autoDrive = true;
        // Opcional: anular input del usuario
        moveInput = new Vector2(0f, autoDriveInput);
    }

    public void StopAutoDrive()
    {
        autoDrive = false;
        moveInput = Vector2.zero;
    }

    void FixedUpdate()
    {
        // Elegir input de aceleración: auto o jugador
        float accel = autoDrive ? autoDriveInput : moveInput.y;
        float motor = accel * drivespeed;
        wheel1.motorTorque = motor;
        wheel2.motorTorque = motor;
        wheel3.motorTorque = motor;
        wheel4.motorTorque = motor;

        // Dirección: recto si es auto-drive
        float steerInput = autoDrive ? 0f : moveInput.x;
        float steer = steerspeed * steerInput;
        wheel1.steerAngle = steer;
        wheel2.steerAngle = steer;
    }
}
