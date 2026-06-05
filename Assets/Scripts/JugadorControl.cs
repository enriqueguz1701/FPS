using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    Coroutine disparar;

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
    [SerializeField] TMP_Text balasTexto, totalBalasTexto;
    Coroutine recargar;

    [Header("Vida")]
    [SerializeField] int vida = 20;
    [SerializeField] Material materialArma;
    [SerializeField] Color rojo = Color.red;
    [SerializeField] Color verde = Color.green;

    [Header("Armas")]
    [SerializeField] GameObject arma1;
    [SerializeField] GameObject arma2;

    // Start is called before the first frame update
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        camara = Camera.main.transform;
        ActualizarUI();
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
        if (context.started && balas > 0 && (arma1.activeInHierarchy || arma2.activeInHierarchy))
        {
            disparar = StartCoroutine(GenerarDisparos());
            if(recargar != null)
            {
                StopCoroutine(recargar);
                recargar = null;
            }
        }

        if (context.canceled)
        {
            if(disparar != null)
            {
                StopCoroutine(disparar);
            }
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

    public IEnumerator GenerarDisparos()
    {
        while (balas > 0)
        {
            balas--;
            if (balas <= 0)
            {
                recargar = StartCoroutine(CargarMunicion());
                break;
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
                else if (raycast.transform.tag == "Enemigo")
                {
                    //Poner el ? es para comprobar que el GetComponent<Enemigo>() no devuelva objeto nulo
                    //Sería igual que si pongo:
                    //Enemigo enemigo = raycast.transform.GetComponent<Enemigo>();
                    //if(enemigo != null)
                    //{
                    //    enemigo.QuitarVida(15);
                    //}
                    raycast.transform.GetComponent<Enemigo>()?.QuitarVida(15);
                }
                else
                {
                    Debug.DrawRay(raycast.point, raycast.normal, Color.blue, 2.0f);

                    Instantiate(efectoDisparo, raycast.point, Quaternion.FromToRotation(Vector3.forward, raycast.normal));
                    //Quaternion rotacion = Quaternion.LookRotation(raycast.normal, Vector3.up);
                    //Instantiate(efectoDisparo, raycast.point, rotacion);

                }
            }

            ActualizarUI();
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("Me quedé sin balas");

    }
    public void Recargar(InputAction.CallbackContext context)
    {
        if (context.started && recargar == null)
        {
            recargar = StartCoroutine(CargarMunicion());
            StopCoroutine(disparar);
            disparar = null;
        }
    }

    IEnumerator CargarMunicion()
    {
        int balasNecesarias = 20 - balas;
        if (totalBalas >= balasNecesarias)
        {
            //balas = 20;
            for (int i = balas; balas < 20; balas++)
            {
                totalBalas--;
                ActualizarUI();
                yield return new WaitForSeconds(0.2f);
            }
            //totalBalas -= balasNecesarias;
        }
        else if (totalBalas > 0)
        {
            //balas = totalBalas;
            for (int i = totalBalas; totalBalas > 0; totalBalas--)
            {
                balas++;
                ActualizarUI();
                yield return new WaitForSeconds(0.2f);
            }
            //totalBalas = 0;
        }
        ActualizarUI();
        recargar = null;
    }

    public void QuitarVida()
    {
        vida--;
        materialArma.color = Color.Lerp(rojo, verde, vida / 20.0f);
    }

    public void ActualizarUI()
    {
        totalBalasTexto.text = totalBalas.ToString();
        balasTexto.text = balas + "/20";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Municion")
        {
            totalBalas += 10;
            ActualizarUI();
            Destroy(other.gameObject);
        }
        else if (other.transform.tag == "Arma")
        {
            Destroy(other.gameObject);
            arma1.SetActive(true);
        }
        else if(other.transform.tag == "Vida")
        {
            vida += 2;
            if(vida >= 20)
            {
                vida = 20;
            }
            materialArma.color = Color.Lerp(rojo, verde, vida / 20.0f);
        }
    }

}
