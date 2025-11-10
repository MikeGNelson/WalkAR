using UnityEngine;
using System.Collections;
using Avoidance;

[RequireComponent(typeof(Animator))] // Ensures the Animator component is attached to the GameObject
public class HeadRotation : MonoBehaviour
{
    public Transform head; // Reference to the head of the character
    public float lookSpeed = 5f; // Speed of rotation when transitioning between modes
    public float minRotationY = -45f; // Minimum Y-axis rotation constraint
    public float maxRotationY = 45f; // Maximum Y-axis rotation constraint
    public bool isLookingAtPlayer = true; // Flag to switch between modes
    public bool canLook = true;
    public Transform player; // Reference to the player
    public float detectionRange = 5f; // Range within which to trigger the animation
    [SerializeField]
    private Animator animator; // Reference to the Animator component
    private bool hasTriggeredAnimation = false; // Flag to track if the animation has been triggered

    // References to the sound and particle system prefab
    [SerializeField]
    private AudioSource sneezeSound; // Reference to the AudioSource for the sneeze sound
    public ParticleSystem sneezeParticlesPrefab; // Reference to the ParticleSystem prefab for the sneeze effect
    public float particleDelay = 0.5f;

    private Quaternion targetRotation; // Store target rotation for the head
    public PlayerController playerController;

    private ParticleSystem pooledSneezeParticles;

    void Start()
    {


        // Ensure the Animator component is automatically attached
        animator = this.GetComponent<Animator>();
        sneezeSound = this.GetComponent<AudioSource>();
        // Find the player dynamically once it spawns
        player = GameObject.FindWithTag("Player").transform;
        playerController = GameObject.FindAnyObjectByType<PlayerController>();
        playerController.eventTriggered = false;

        //if (sneezeParticlesPrefab != null)
        //{
        //    pooledSneezeParticles = Instantiate(sneezeParticlesPrefab);
        //    pooledSneezeParticles.gameObject.SetActive(false);
        //}

        GameObject sneezeObj = GameObject.FindWithTag("sneeze_particles");
        if (sneezeObj != null)
        {
            pooledSneezeParticles = sneezeObj.GetComponent<ParticleSystem>();
            //pooledSneezeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            pooledSneezeParticles.Play();
            pooledSneezeParticles.gameObject.SetActive(false); // optional if it's already off
        }
        else
        {
            Debug.LogWarning("Sneeze particle system not found! Make sure it has the 'sneeze_particles' tag.");
        }

        animator.Play("Sneeze", 0, 1f); // Play sneeze and skip to end
        animator.Update(0); // Force animator to process the frame

        // Reset to idle immediately
        animator.Play("HumanoidIdle");
    }

    void Update()
    {
        if (player != null && canLook)  // Ensure the player is found
        {
            // Check distance to player and trigger animation if within range
            float distanceToPlayer = Vector3.Distance(head.position, player.position);
            if (distanceToPlayer <= detectionRange && !hasTriggeredAnimation)
            {
                playerController.eventTriggered = true;
                TriggerAnimation(true); // Trigger the "Sneeze" animation when within range
            }
            else if (distanceToPlayer > detectionRange && hasTriggeredAnimation)
            {
                TriggerAnimation(false); // Optionally stop the animation when out of range
            }

            //// Only allow the head to look at or away from the player after the animation is triggered
            //if (hasTriggeredAnimation)
            //{
            //    if (isLookingAtPlayer)
            //    {
            //        LookAtPlayer();
            //    }
            //    else
            //    {
            //        LookAwayFromPlayer();
            //    }
            //}
        }
    }

    void TriggerAnimation(bool isInRange)
    {
        if (isInRange && !hasTriggeredAnimation)
        {
            // Trigger the "Sneeze" animation
            animator.SetTrigger("Sneeze");
            hasTriggeredAnimation = true; // Set the flag to true once the animation is triggered

            // Play the sneeze sound
            if (sneezeSound != null)
            {
                sneezeSound.Play();
            }

            // Start the coroutine to spawn the particle system after the head has moved
            StartCoroutine(SpawnParticleSystemAfterHeadMovement());
        }
        //else
        //{
        //    // Optionally stop the animation or reset logic here
        //    hasTriggeredAnimation = false; // Reset the flag if out of range
        //}
    }

    // Coroutine to spawn the particle system after the head has moved
    IEnumerator SpawnParticleSystemAfterHeadMovement()
    {
        // Wait for the head to finish its rotation (adjust the wait time as needed)
        yield return new WaitForSeconds(particleDelay); // You may need to tweak this wait time
        yield return null; // Delay one more frame

        //// Spawn the particle system at the head's position and apply the final rotation
        //if (sneezeParticlesPrefab != null)
        //{
        //    ParticleSystem sneezeParticles = Instantiate(sneezeParticlesPrefab, head.position, Quaternion.identity);

        //    // Set the particle system to world space
        //    var main = sneezeParticles.main;
        //    main.simulationSpace = ParticleSystemSimulationSpace.World;

        //    // Apply the rotation to the particle system after the head has rotated
        //    sneezeParticles.transform.rotation = targetRotation;

        //    // Play the sneeze particle system
        //    sneezeParticles.Play();
        //}

        if (pooledSneezeParticles != null)
        {
            var main = pooledSneezeParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            pooledSneezeParticles.transform.position = head.position;
            pooledSneezeParticles.transform.rotation = targetRotation;

            pooledSneezeParticles.gameObject.SetActive(true);
            pooledSneezeParticles.Play();
        }
    }

    // Makes the head look at the player
    void LookAtPlayer()
    {
        // Calculate the direction from the head to the player
        Vector3 direction = player.position - head.position;

        // Make sure the head only rotates on the y-axis (to keep a realistic head movement)
        direction.y = 0;

        // Calculate the desired rotation to look at the player
        targetRotation = Quaternion.LookRotation(direction);

        targetRotation = Quaternion.Euler(
            head.rotation.eulerAngles.x,
            Mathf.Clamp(targetRotation.eulerAngles.y, minRotationY, maxRotationY),
            head.rotation.eulerAngles.z
        );

        // Apply the rotation with constraints
        head.rotation = targetRotation;

        // Smoothly interpolate the current rotation to the target rotation
        head.rotation = Quaternion.Slerp(head.rotation, targetRotation, Time.deltaTime * lookSpeed);
    }

    // Makes the head look away from the player
    void LookAwayFromPlayer()
    {
        // Calculate the direction opposite to the player
        Vector3 direction = head.position - player.position;

        // Make sure the head only rotates on the y-axis (to keep a realistic head movement)
        direction.y = 0;

        // Calculate the desired rotation to look away from the player
        targetRotation = Quaternion.LookRotation(direction);

        targetRotation = Quaternion.Euler(
            head.rotation.eulerAngles.x,
            Mathf.Clamp(targetRotation.eulerAngles.y, minRotationY, maxRotationY),
            head.rotation.eulerAngles.z
        );

        // Apply the rotation with constraints
        head.rotation = targetRotation;

        // Smoothly interpolate the current rotation to the target rotation
        head.rotation = Quaternion.Slerp(head.rotation, targetRotation, Time.deltaTime * lookSpeed);
    }

    // Apply rotation after animations are applied
    void LateUpdate()
    {
        // Only update the head rotation after the animation is done
        if (player != null && hasTriggeredAnimation)
        {
            if (isLookingAtPlayer)
            {
                LookAtPlayer();
            }
            else
            {
                LookAwayFromPlayer();
            }
        }
    }

    // Toggle between looking at the player and looking away
    public void ToggleHeadMode()
    {
        isLookingAtPlayer = !isLookingAtPlayer;
    }
}
