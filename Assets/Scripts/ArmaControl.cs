using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArmaControl : MonoBehaviour
{
    [Header("Disparo")]
    [SerializeField] RaycastHit raycast;
    [SerializeField] Transform camara;
    [SerializeField] float distanciaDisparo;
    [SerializeField] ParticleSystem particulasDisparo;
    [SerializeField] bool esAutomatica;

    Coroutine disparar;

    [Header("Partículas")]
    [SerializeField] GameObject explosion;
    [SerializeField] GameObject efectoDisparo;

    [Header("Cargador")]
    [SerializeField] int balas = 20;
    [SerializeField] int totalBalas = 100;
    [SerializeField] TMP_Text balasTexto, totalBalasTexto;
    Coroutine recargar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camara = Camera.main.transform;
        ActualizarUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Disparar(InputAction.CallbackContext context)
    {
        if (context.started && balas > 0 && gameObject.activeInHierarchy)
        {
            if (esAutomatica)
            {
                disparar = StartCoroutine(GenerarDisparosAutomaticos());
            }
            else
            {
                GenerarDisparos();
                
            }
            if (recargar != null)
            {
                StopCoroutine(recargar);
                recargar = null;
            }
        }

        if (context.canceled)
        {
            if (disparar != null)
            {
                StopCoroutine(disparar);
            }
        }
    }

    void GenerarDisparos()
    {
        balas--;
        Debug.DrawRay(camara.position, camara.forward, Color.red, 2.0f);
        particulasDisparo.Play();
        if (Physics.Raycast(camara.position, camara.forward, out raycast, distanciaDisparo))
        {
            if (raycast.transform.tag == "Destruir")
            {
                Destroy(raycast.transform.gameObject);
                Instantiate(explosion, raycast.transform.position, raycast.transform.rotation);
            }
            else if (raycast.transform.tag == "Cabeza")
            {
                raycast.transform.parent.GetComponent<Enemigo>()?.QuitarVida(15000000);
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
    }

    public IEnumerator GenerarDisparosAutomaticos()
    {
        while (balas > 0)
        {
           GenerarDisparos();
            if (balas <= 0)
            {
                recargar = StartCoroutine(CargarMunicion());
                break;
            }
           
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("Me quedé sin balas");

    }
    public void Recargar(InputAction.CallbackContext context)
    {
        if (context.started && recargar == null && gameObject.activeInHierarchy)
        {
            recargar = StartCoroutine(CargarMunicion());
            if(disparar != null)
            {
                StopCoroutine(disparar);
                disparar = null;
            }
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
                Debug.Log(balas);
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

    public void ActualizarUI()
    {
        totalBalasTexto.text = totalBalas.ToString();
        balasTexto.text = balas + "/20";
    }

    public void AumentarMunicion(int balas)
    {
        totalBalas += balas;
        ActualizarUI();
    }
}
