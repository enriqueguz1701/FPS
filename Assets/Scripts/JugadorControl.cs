using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class JugadorControl : MonoBehaviour
{
    [SerializeField] CharacterController characterController;

    [Header("Movimiento")]
    [SerializeField] float velocidad;
    [SerializeField] Vector2 direccion;
    [SerializeField] Vector3 vectorMovimiento;
    [SerializeField] Vector3 movimientoVertical;
    [SerializeField] float alturaSalto, gravedad;

  
  
    [Header("Suelo")]
    [SerializeField] bool estaEnSuelo;
    [SerializeField] Vector3 abajo;
    [SerializeField] float radioEsfera;
    [SerializeField] LayerMask layerSuelo;

  

    [Header("Vida")]
    [SerializeField] int vida = 20;
    [SerializeField] Material[] materialArma;
    [SerializeField] Image imagenNegro;
    [SerializeField] int armaActiva;
    [SerializeField] Color rojo = Color.red;
    [SerializeField] Color verde = Color.green;

    [Header("Armas")]
    [SerializeField] GameObject arma1;
    [SerializeField] bool tengoArma1, tengoArma2;
    [SerializeField] GameObject arma2;

    // Start is called before the first frame update
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        ActualizarUI();
    }

    // Update is called once per frame
    void Update()
    {
        vectorMovimiento = transform.right * direccion.x + transform.forward * direccion.y;
        characterController.Move(vectorMovimiento * velocidad * Time.deltaTime);

        movimientoVertical.y -= gravedad * Time.deltaTime;
        characterController.Move(movimientoVertical * Time.deltaTime);

        //materialArma.color = Color.Lerp(rojo, verde, vida / 20.0f);
    }

    private void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + abajo, radioEsfera, layerSuelo);
        estaEnSuelo = colliders.Length > 0;
    }

    public void Movimiento(InputAction.CallbackContext value)
    {
        direccion = value.ReadValue<Vector2>();
    }

    public void Saltar(InputAction.CallbackContext context)
    {
        if (context.started && estaEnSuelo)
        {
            movimientoVertical.y = Mathf.Sqrt(2 * gravedad * alturaSalto);
        }
    }

    public void Siguiente(InputAction.CallbackContext context)
    {
        if (context.started && tengoArma1)
        {
            arma1.SetActive(true);
            arma2.SetActive(false);
        }
    }

    public void Anterior(InputAction.CallbackContext context)
    {
        if (context.started && tengoArma2)
        {
            arma1.SetActive(false);
            arma2.SetActive(true);
        }
    }

    public void Correr(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            velocidad += 5;
        }
        if (context.canceled)
        {
            velocidad -= 5;
        }
    }

 
    public void QuitarVida()
    {
        vida--;
        ActualizarUI();
    }

    void ActualizarUI()
    {
        materialArma[armaActiva].color = Color.Lerp(rojo, verde, vida / 20.0f);
        Debug.Log("Porcentaje negro: " + Mathf.Lerp(1, 0, vida / 20f));
        imagenNegro.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, vida / 20f));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Municion")
        {
            //Con esta forma recargo balas en el arma que tengo activo
            GetComponentInChildren<ArmaControl>().AumentarMunicion(10);
            Destroy(other.gameObject);
        }
        else if (other.transform.tag == "Arma")
        {
            Destroy(other.gameObject);
            arma1.SetActive(true);
            arma2.SetActive(false);
            tengoArma1 = true;
            armaActiva = 0;
        }
        else if(other.transform.tag == "Vida")
        {
            vida += 2;
            if(vida >= 20)
            {
                vida = 20;
            }
            ActualizarUI();
        }
    }

}
