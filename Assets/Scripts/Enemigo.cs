using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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

    [SerializeField] int vida = 100;
    [SerializeField] Image barraVida;
    [SerializeField] float escala;
    [SerializeField] GameObject[] objetosCrear;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        puntoActual = 0;    
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(puntos[puntoActual].position);

        escala = barraVida.rectTransform.sizeDelta.x / vida;

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
                atacando = true;
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
            
            if (persiguiendoJugador && agent.remainingDistance <= agent.stoppingDistance)
            {
                    agent.SetDestination(puntos[puntoActual].position);
                    persiguiendoJugador = false;                
            }
            
            if(!persiguiendoJugador && agent.remainingDistance <= agent.stoppingDistance)
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

    private void LateUpdate()
    {
        barraVida.transform.parent.LookAt(Camera.main.transform.position);  
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
      
        JugadorControl jugadorControl = jugador.GetComponent<JugadorControl>(); 
        while (true)
        {
            yield return new WaitForSeconds(1);
            transform.LookAt(jugador);
            jugadorControl.QuitarVida();
        }
        
    }

    public void QuitarVida(int cantidadARestar)
    {
        vida-=cantidadARestar;
        barraVida.rectTransform.sizeDelta = new Vector2(vida * escala, barraVida.rectTransform.sizeDelta.y);
        if(vida <= 0)
        {
            float probabilidad = Random.Range(0f, 1f);
            Debug.Log("Probabilidad de soltar " +  probabilidad);   
            if(probabilidad >= 0.4)
            {
                Instantiate(objetosCrear[Random.Range(0, objetosCrear.Length)], transform.position, Quaternion.identity);
            }

            Destroy(gameObject);    
        }
    }
}
