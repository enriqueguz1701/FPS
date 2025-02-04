using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamaraControl : MonoBehaviour
{
    [SerializeField] Vector2 direccion;
    [SerializeField] float rotacionY, minimo, maximo;
    // Start is called before the first frame update
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        rotacionY -= direccion.y;
        rotacionY = Mathf.Clamp(rotacionY, minimo, maximo);
        transform.localRotation = Quaternion.Euler(rotacionY, 0, 0);
        transform.parent.Rotate(Vector3.up * direccion.x);
    }

    public void Movimiento(InputAction.CallbackContext value)
    {
        direccion = value.ReadValue<Vector2>();
    }
}
