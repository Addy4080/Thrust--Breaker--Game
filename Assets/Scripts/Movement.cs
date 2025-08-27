using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] InputAction Thrust;
    [SerializeField] InputAction Rotation;
    [SerializeField] float thrustStrength;
    [SerializeField] float rotationStrengh;
    [SerializeField] ParticleSystem mainThruster;
    [SerializeField] ParticleSystem leftThruster;
    [SerializeField] ParticleSystem rightThruster;


    Rigidbody rb;
    AudioSource audioSource;

    public bool isFrozen = false;

    [SerializeField] AudioClip rocketThrustSound;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        Thrust.Enable();
        Rotation.Enable();
    }

    void FixedUpdate()
    {
        if (isFrozen) return;
        ProcessThrust();
        ProcessRotation();
    }

    private void ProcessThrust()
    {
        if (Thrust.IsPressed())
        {
            rb.AddRelativeForce(Vector3.up * thrustStrength * Time.fixedDeltaTime);

            if (!audioSource.isPlaying)
            {
                audioSource.clip = rocketThrustSound;
                audioSource.Play();
                mainThruster.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying && audioSource.clip == rocketThrustSound)
            {
                audioSource.Stop();
                mainThruster.Stop();
            }
        
        }
    }

    private void ProcessRotation()
    {
        float RotationInput = Rotation.ReadValue<float>();
        if (RotationInput < 0)
        {

            ApplyRotation(rotationStrengh);
            
            if (!rightThruster.isPlaying)
            {
                leftThruster.Stop();
                rightThruster.Play();
            }

        }
        else if (RotationInput > 0)
        {
            ApplyRotation(-rotationStrengh);

            if (!leftThruster.isPlaying)
            {
                rightThruster.Stop();
                leftThruster.Play();
            }

        }
        else
        {
            leftThruster.Stop();
            rightThruster.Stop();
        }
    }

    private void ApplyRotation(float rotationStrength)
    {
        rb.freezeRotation = true;
        transform.Rotate(Vector3.forward * rotationStrength * Time.fixedDeltaTime);
        rb.freezeRotation = false;
    }

    public void FreezeControls(float duration)
    {
        isFrozen = true;
        audioSource.Stop();
        Invoke(nameof(UnfreezeControls), duration);
    }

    private void UnfreezeControls()
    {
        isFrozen = false;
    }
}
