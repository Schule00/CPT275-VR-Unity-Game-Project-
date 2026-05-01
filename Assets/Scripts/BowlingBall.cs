using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class BowlingBall : MonoBehaviour
{
    [Header("Respawn Settings")]
    public float respawnDelay = 7f;
    public float throwSpeedThreshold = 1.2f;

    [Header("Respawn Position (set this manually in Inspector!)")]
    [Tooltip("Drag the ball to where you want it to spawn, then click 'Save Current Position' by right-clicking this script, OR just manually fill in these values.")]
    public Vector3 spawnPosition;
    public Vector3 spawnRotationEuler;

    private Rigidbody _rb;
    private XRGrabInteractable _grab;

    private bool _isHeld = false;
    private bool _wasThrown = false;
    private bool _respawnQueued = false;
    private bool _throwStarted = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _grab = GetComponent<XRGrabInteractable>();
    }

    private void Start()
    {
        // If no manual spawn position set, fall back to current position at Start
        // (Start runs after Awake so the scene is more settled)
        if (spawnPosition == Vector3.zero)
        {
            spawnPosition = transform.position;
            spawnRotationEuler = transform.eulerAngles;
        }
    }

    private void OnEnable()
    {
        _grab.selectEntered.AddListener(OnGrabbed);
        _grab.selectExited.AddListener(OnReleased);
    }

    private void OnDisable()
    {
        _grab.selectEntered.RemoveListener(OnGrabbed);
        _grab.selectExited.RemoveListener(OnReleased);
    }

    private void Update()
    {
        if (_isHeld || !_wasThrown || _respawnQueued)
            return;

        if (!_throwStarted && _rb.linearVelocity.magnitude >= throwSpeedThreshold)
        {
            _throwStarted = true;

            if (BowlingScoreboard.Instance != null)
                BowlingScoreboard.Instance.StartNewThrow();

            _respawnQueued = true;
            StartCoroutine(RespawnAfterDelay());
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        _isHeld = true;
        _wasThrown = false;
        _throwStarted = false;
        StopAllCoroutines();
        _respawnQueued = false;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        _isHeld = false;
        _wasThrown = true;
        _throwStarted = false;
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
    }

    private void Respawn()
    {
        // Stop all movement
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        // Move to saved spawn
        transform.position = spawnPosition;
        transform.rotation = Quaternion.Euler(spawnRotationEuler);

        // Small delay before re-enabling physics so it doesn't
        // immediately get flung by residual forces or gravity spike
        StartCoroutine(ReEnablePhysics());
    }

    private IEnumerator ReEnablePhysics()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        _rb.isKinematic = false;

        _wasThrown = false;
        _respawnQueued = false;
        _throwStarted = false;
    }

    // ── Editor helper ─────────────────────────────────────────────────────────
    /// <summary>
    /// Call this from the Inspector context menu to lock in the current
    /// position as the spawn point without needing to type coordinates.
    /// </summary>
    [ContextMenu("Save Current Position as Spawn")]
    private void SaveCurrentPositionAsSpawn()
    {
        spawnPosition = transform.position;
        spawnRotationEuler = transform.eulerAngles;
        Debug.Log($"Spawn position saved: {spawnPosition}");
    }
}