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
    [SerializeField] float distanciaVision, anguloVision, distanciaAtacarJugador;
    [SerializeField] bool atacando;

    Coroutine corrutinaAtacar;
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

        if (PuedeVerJugador())
        {
            persiguiendoJugador = true;
            agent.SetDestination(jugador.position);
            if(agent.remainingDistance < distanciaAtacarJugador && !atacando)
            {
                corrutinaAtacar = StartCoroutine(Atacar());
            }
            else if(agent.remainingDistance >= distanciaAtacarJugador)
            {
                if (corrutinaAtacar != null)
                {
                    StopCoroutine(corrutinaAtacar);
                }
                atacando = false;
            }
        }
        else
        {
            if(corrutinaAtacar != null)
            {
                StopCoroutine(corrutinaAtacar);
            }
            
            if (persiguiendoJugador)
            {
                agent.SetDestination(puntos[puntoActual].position);
            }
            persiguiendoJugador = false;
            if(agent.remainingDistance <= agent.stoppingDistance)
            {
                puntoActual++;
                if (puntoActual == puntos.Length)
                {
                    puntoActual = 0;
                }
                agent.SetDestination(puntos[puntoActual].position);
            }
        }
    }

    public bool PuedeVerJugador()
    {
        //Calculo la dirección en la que está el jugador restando su posición
        //menos la del enemigo
        Vector3 direccionJugador = jugador.position - transform.position;
        //Como dirección es un vector y necesito un número tengo que calcular la 
        //magnitud de ese vector (raíz cuadrada de x^2 + y^2 + z^2)
        float distancia = direccionJugador.magnitude;

        //Si esa distancia es más grande que el campo de visión del enemigo quiere
        //decir que no lo está viendo
        if (distancia > distanciaVision)
            return false;

        //Ahora calculo el ángulo que hay entre el enemigo y el jugador
        float angulo = Vector3.Angle(transform.forward, direccionJugador);

        //Si ese ángulo es más grande que el ángulo de visión del enemigo
        //quiere decir que no lo está viendo
        if (angulo > anguloVision / 2f)
            return false;

        Debug.DrawRay(transform.position, direccionJugador.normalized, Color.red, 5);
        //En esta parte el jugador ya está en el campo de visión del enemigo, pero puede haber obstáculos
        //que impiden que el enemigo vea al jugador
        //Para comprobar si hay obstáculos disparamos un rayo desde el enemigo hacia el jugador
        if (Physics.Raycast(transform.position, direccionJugador.normalized, out RaycastHit hit, distancia))
        {
            //Si el objeto con el que impacta el rayo es el jugador devolvemos verdadero
            return hit.transform.tag == "Player";
        }

        //Si el objeto con el que impacta el rayo no es el jugador devolvemos falso
        return false;
    }

    IEnumerator Atacar()
    {
        Debug.Log("Comienzo a atacar");
        atacando = true;
        JugadorControl jugadorControl = jugador.GetComponent<JugadorControl>(); 
        while (true)
        {
            yield return new WaitForSeconds(1);
            Debug.Log("Ataco al jugador");
            jugadorControl.QuitarVida();
        }
        
    }
}
