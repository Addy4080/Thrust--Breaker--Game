using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class CollisionHandeler : MonoBehaviour
{
    [SerializeField] float levelLoadDelayTime = 0f;
    [SerializeField] AudioClip CrashSFX;
    [SerializeField] AudioClip SuccessSFX;
    [SerializeField] AudioClip ShieldPickupSFX;
    [SerializeField] AudioClip ShieldBreakSFX;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem crashParticles;
    [SerializeField] GameObject shieldVisuals;
    [SerializeField] float invincibilityTime = 1f;
    [SerializeField] float shieldBounceForce = 0.5f;
    [SerializeField] Transform cameraTransform;
    [SerializeField] float freezeDuration = 0.5f; // adjustable in Inspector

    [SerializeField] float volumeScale;
    Rigidbody rb;
    Movement movementScript;
    AudioSource audioSource;

    bool isControllable = true;
    bool isCollidable = true;
    bool isInvincible = false;   // FIX: renamed spelling to match
    bool hasShield = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        movementScript = GetComponent<Movement>();
    }

    void Update()
    {
        RespondToDebugKeys();
    }

    private void RespondToDebugKeys()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            NextLevel();
        }
        else if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            isCollidable = !isCollidable;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Shield":
                ShieldSequence(other.gameObject);
               
                break;
            default:
                break;
        }
    }

    void OnCollisionEnter(Collision other)
    {
         if (!isControllable || !isCollidable) { return; }

            if (hasShield && other.gameObject.tag != "Friendly" && other.gameObject.tag != "Finish" && other.gameObject.tag != "Shield")
        {
            ShieldAbsorbHitSequence();


            // Start invincibility (ends properly)
            StartCoroutine(InvincibilityDelay());

            // --- Bounce along LOCAL X axis (both + and -) ---
            BounceBackSequence(other);
            return;

        }

        switch (other.gameObject.tag)
        {
            case "Friendly":
                Debug.Log("hi friend");
                break;
            case "Finish":
                SuccessSequence();
                break;
            default:
                StartCrashSequence();
                break;
        }
    }

    private void ShieldAbsorbHitSequence()
    {
        Debug.Log("Shield absorbed the hit!");
        AudioSource.PlayClipAtPoint(ShieldBreakSFX, shieldVisuals.transform.position, volumeScale);
        // Turn off shield
        hasShield = false;
        shieldVisuals.SetActive(false);
         
        
            
        
    }

    private void BounceBackSequence(Collision other)
    {
        rb.constraints = RigidbodyConstraints.None; // allow free motion
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Decide bounce direction (local X positive or negative)
        Vector3 localCollision = transform.InverseTransformDirection(other.contacts[0].normal);
        Vector3 bounceDir = (localCollision.x > 0f) ? transform.right : -transform.right;

        // Apply bounce
        rb.AddForce(bounceDir * shieldBounceForce, ForceMode.Impulse);

        // Restrict velocity strictly to local X axis
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        localVel.y = 0f; // remove Y
        localVel.z = 0f; // remove Z
        rb.linearVelocity = transform.TransformDirection(localVel);

        // Extra: lock Z position to prevent drift
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;

        // Disable player control
        isControllable = false;
        if (movementScript != null) movementScript.FreezeControls(freezeDuration);

        // Camera shake
        StartCoroutine(CameraShake(0.2f, 0.1f));

        // Restore after short delay
        StartCoroutine(RestoreControl());
        return;
    }



    private void ShieldSequence(GameObject powerUp)
    {
        
               
       
        hasShield = true;
        shieldVisuals.SetActive(true);
        AudioSource.PlayClipAtPoint(ShieldPickupSFX, powerUp.transform.position, volumeScale);
        Destroy(powerUp);
        
    }

    void StartCrashSequence()
    {
        if (isInvincible) return;  // FIX: was staying true forever
        isControllable = false;
        audioSource.Stop();
        crashParticles.Play();
        audioSource.PlayOneShot(CrashSFX);
        GetComponent<Movement>().enabled = false;
        Invoke("ReloadLevel", levelLoadDelayTime);
    }

    void SuccessSequence()
    {
        isControllable = false;
        audioSource.Stop();
        audioSource.PlayOneShot(SuccessSFX);
        successParticles.Play();
        GetComponent<Movement>().enabled = false;
        Invoke("NextLevel", levelLoadDelayTime);
    }

    private void NextLevel()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int nextScene = currentScene + 1;
        if (nextScene == SceneManager.sceneCountInBuildSettings)
        {
            nextScene = 0;
        }
        SceneManager.LoadScene(nextScene);
    }

    private void ReloadLevel()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }

    IEnumerator InvincibilityDelay()
    {
        isInvincible = true;
        Debug.Log("Invincibility ON");
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
        Debug.Log("Invincibility OFF");
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            cameraTransform.localPosition = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cameraTransform.localPosition = originalPos;
    }

   IEnumerator RestoreControl()
{
    // Save current Y position so rocket doesn't fall during freeze
    float fixedY = transform.position.y;

    float timer = 0f;
    while (timer < freezeDuration)
    {
        // Keep Y fixed while still letting X bounce happen
        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;

        timer += Time.deltaTime;
        yield return null;
    }

    rb.constraints = RigidbodyConstraints.FreezeRotation;
    isControllable = true;
}

}
