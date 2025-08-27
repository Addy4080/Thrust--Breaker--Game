using UnityEngine;

public class Oscillator : MonoBehaviour
{
    Vector3 startPosititon;
    Vector3 endPosititon;

    [SerializeField] Vector3 movementVector;

    [SerializeField] float speed;

    float movementfactor;


    void Start()
    {
        startPosititon = transform.position;
        endPosititon = startPosititon + movementVector; 
    }

    void Update()
    {
        movementfactor = Mathf.PingPong(Time.time * speed, 1f);
        transform.position = Vector3.Lerp(startPosititon, endPosititon, movementfactor);
    }
}
