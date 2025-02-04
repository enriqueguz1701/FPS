using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
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

    [Header("Disparo")]
    [SerializeField] RaycastHit raycast;
    [SerializeField] Transform camara;
    [SerializeField] float distanciaDisparo;
    [SerializeField] ParticleSystem particulasDisparo;

    [Header("Partículas")]
    [SerializeField] GameObject explosion;
    [SerializeField] GameObject efectoDisparo;

    [Header("Suelo")]
    [SerializeField] bool estaEnSuelo;
    [SerializeField] Vector3 abajo;
    [SerializeField] float radioEsfera;
    [SerializeField] LayerMask layerSuelo;

    [Header("Cargador")]
    [SerializeField] int balas = 20;
    [SerializeField] int totalBalas = 100;

    [Header("Vida")]
    [SerializeField] int vida = 20;
    [SerializeField] Material materialArma;
    [SerializeField] Color rojo = Color.red;
    [SerializeField] Color verde = Color.green;

    // Start is called before the first frame update
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        camara = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        vectorMovimiento = transform.right * direccion.x + transform.forward * direccion.y;
        characterController.Move(vectorMovimiento * velocidad * Time.deltaTime);

        movimientoVertical.y -= gravedad * Time.deltaTime;
        characterController.Move(movimientoVertical * Time.deltaTime);

        materialArma.color = Color.Lerp(rojo, verde, vida / 20.0f);
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

    public void Disparar(InputAction.CallbackContext context)
    {
        if (context.started && balas > 0)
        {
            StartCoroutine(GenerarDisparos());
        }

        if (context.canceled)
        {
            StopAllCoroutines();
        }
    }

    public IEnumerator GenerarDisparos()
    {
        while(balas > 0)
        {
            balas--;
            if (balas <= 0)
            {
                CargarMunicion();
            }
            Debug.DrawRay(camara.position, camara.forward, Color.red, 2.0f);
            particulasDisparo.Play();
            if (Physics.Raycast(camara.position, camara.forward, out raycast, distanciaDisparo))
            {
                if (raycast.transform.tag == "Destruir")
                {
                    Destroy(raycast.transform.gameObject);
                    Instantiate(explosion, raycast.transform.position, raycast.transform.rotation);
                }
                else
                {
                    Debug.DrawRay(raycast.point, raycast.normal, Color.blue, 2.0f);

                    Instantiate(efectoDisparo, raycast.point, Quaternion.FromToRotation(Vector3.forward, raycast.normal));
                    //Quaternion rotacion = Quaternion.LookRotation(raycast.normal, Vector3.up);
                    //Instantiate(efectoDisparo, raycast.point, rotacion);

                }
            }
            yield return new WaitForSeconds(0.5f);
        }
        
    }
    public void Recargar(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            CargarMunicion();
        }
    }

    void CargarMunicion()
    {
        int balasNecesarias = 20 - balas;
        if(totalBalas >= balasNecesarias)
        {
            balas = 20;
            totalBalas -= balasNecesarias;
        }else if(totalBalas > 0)
        {
            balas = totalBalas;
            totalBalas = 0;
        }
    }

    public void QuitarVida()
    {
        vida--;
        materialArma.color = Color.Lerp(rojo, verde, vida / 20.0f);
    }
}
