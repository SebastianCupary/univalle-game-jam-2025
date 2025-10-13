using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class car : MonoBehaviour
{
    public Rigidbody rigid;
    public WheelCollider wheel1, wheel2, wheel3, wheel4;
    public float drivespeed, steerspeed;

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

    void Update()
    {
        // No need to poll input here, handled by events
    }

    void FixedUpdate()
    {
        float motor = moveInput.y * drivespeed;
        wheel1.motorTorque = motor;
        wheel2.motorTorque = motor;
        wheel3.motorTorque = motor;
        wheel4.motorTorque = motor;
        float steer = steerspeed * moveInput.x;
        wheel1.steerAngle = steer;
        wheel2.steerAngle = steer;
    }
}
