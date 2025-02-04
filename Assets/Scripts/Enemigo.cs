using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemigo : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform[] puntos;
    [SerializeField] int puntoActual;

    [SerializeField] Transform jugador;
    [SerializeField] bool persiguiendoJugador;

    [SerializeField] RaycastHit raycast;
    [SerializeField] float campoVision;
    [SerializeField] bool atacando;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        puntoActual = 0;    
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(puntos[puntoActual].position);

        jugador = FindAnyObjectByType<JugadorControl>().transform;
    }

    // Update is called once per frame
    void Update()
    {

        if (!persiguiendoJugador && agent.remainingDistance <= agent.stoppingDistance)
        {
            puntoActual++;
            if(puntoActual == puntos.Length)
            {
                puntoActual = 0;
            }
            agent.SetDestination(puntos[puntoActual].position);
        }
        

        Debug.DrawRay(transform.position, transform.forward, Color.red, 5);
        if (Physics.Raycast(transform.position, transform.forward, out raycast, campoVision))
        {
            if (raycast.transform.tag == "Player")
            {
                Debug.Log("He visto al jugador");
                persiguiendoJugador = true;
                agent.SetDestination(jugador.position); 
                if (!atacando)
                {
                    StartCoroutine(Atacar());
                }
            }
            else
            {
                persiguiendoJugador = false;
                atacando = false;
                agent.SetDestination(puntos[puntoActual].position);
                StopAllCoroutines();
            }
        }
        else
        {
            persiguiendoJugador = false;
            atacando = false;
            agent.SetDestination(puntos[puntoActual].position);
            StopAllCoroutines();
        }
    }

    IEnumerator Atacar()
    {
        atacando = true;
        JugadorControl jugadorControl = jugador.GetComponent<JugadorControl>(); 
        while (true)
        {
            yield return new WaitForSeconds(1);
            jugadorControl.QuitarVida();
        }
        
    }
}
