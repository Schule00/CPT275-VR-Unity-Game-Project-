using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SlotMachineLever : MonoBehaviour
{
    [Header("References")]
    public Transform leverRoot;          // The LeverRoot that rotates
    public XRGrabInteractable grabInteractable; // On the sphere

    [Header("Lever Settings")]
    public float maxPullAngle = 75f;     // Max degrees it pulls down
    public float springSpeed = 6f;       // Speed it snaps back
    public float pullThreshold = 60f;    // Angle that counts as "pulled"

    private bool isGrabbed = false;
    private bool hasTriggered = false;
    private float currentAngle = 0f;
    private Transform handTransform;

    // Store the lever's starting world position for reference
    private Vector3 leverStartWorldPos;

    void Start()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        leverStartWorldPos = leverRoot.position;
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        hasTriggered = false;
        handTransform = args.interactorObject.transform;
        Debug.Log("Grabbed!");
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        handTransform = null;
        Debug.Log("Released! Springing back...");
    }

    void Update()
    {
        if (isGrabbed && handTransform != null)
        {
            // Get hand position relative to the lever root
            Vector3 localHand = leverRoot.InverseTransformPoint(handTransform.position);

            // Pull angle is based on how LOW the hand is
            // Negative Y = hand is below the pivot = pulling down
            float targetAngle = Mathf.Clamp(localHand.y * -90f, 0f, maxPullAngle);
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * 25f);

            // Check if fully pulled
            if (currentAngle >= pullThreshold && !hasTriggered)
            {
                hasTriggered = true;
                OnLeverFullyPulled();
            }
        }
        else
        {
            // Spring back smoothly to upright (0 degrees)
            currentAngle = Mathf.Lerp(currentAngle, 0f, Time.deltaTime * springSpeed);
        }

        // Apply the rotation to the lever root — X axis = tilt forward/back
        leverRoot.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);
    }

    void OnLeverFullyPulled()
    {
        Debug.Log("🎰 LEVER FULLY PULLED — SPIN!");
        // Hook your slot machine spin here:
        // SlotMachineManager.Instance.Spin();
    }
}