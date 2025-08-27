using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    [SerializeField] float rotationSpeed;
    float rotateY = 1f;

    void Update()
    {
        transform.Rotate(0f, rotateY * rotationSpeed * Time.deltaTime,0f); 
    }
}
