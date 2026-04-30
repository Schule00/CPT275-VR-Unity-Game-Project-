using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Attach to the Sphere (bowling ball).
/// Requires: Rigidbody, SphereCollider, XRGrabInteractable.
/// Tag the Sphere as "BowlingBall" in Edit > Project Settings > Tags.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class BowlingBall : MonoBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("Seconds after the ball is thrown before it respawns.")]
    public float respawnDelay = 7f;

    [Tooltip("Minimum speed (m/s) after release to count as a real throw.")]
    public float throwSpeedThreshold = 0.5f;

    // ── Private state ────────────────────────────────────────────────────────
    private Rigidbody _rb;
    private XRGrabInteractable _grab;

    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private bool _isHeld = false;
    private bool _wasThrown = false;
    private bool _respawnQueued = false;
    private bool _throwStarted = false;   // did we already fire StartNewThrow this release?

    // ── Unity lifecycle ──────────────────────────────────────────────────────
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _grab = GetComponent<XRGrabInteractable>();
        _startPosition = transform.position;
        _startRotation = transform.rotation;
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

        // Wait until the ball is actually moving before registering the throw.
        // This avoids counting a "throw" if the player just drops the ball.
        if (!_throwStarted && _rb.linearVelocity.magnitude >= throwSpeedThreshold)
        {
            _throwStarted = true;

            // A real throw — tell the scoreboard now so pins are unlocked.
            if (BowlingScoreboard.Instance != null)
                BowlingScoreboard.Instance.StartNewThrow();

            _respawnQueued = true;
            StartCoroutine(RespawnAfterDelay());
        }
    }

    // ── XR callbacks ─────────────────────────────────────────────────────────
    private void OnGrabbed(SelectEnterEventArgs args)
    {
        _isHeld = true;
        _wasThrown = false;
        _throwStarted = false;

        // Cancel any pending auto-respawn.
        StopAllCoroutines();
        _respawnQueued = false;

        // No scoreboard call here — throw is registered on release+movement.
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        _isHeld = false;
        _wasThrown = true;
        _throwStarted = false;   // arm the speed check in Update
    }

    // ── Respawn logic ────────────────────────────────────────────────────────
    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
    }

    private void Respawn()
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;

        transform.position = _startPosition;
        transform.rotation = _startRotation;

        _rb.isKinematic = false;

        _wasThrown = false;
        _respawnQueued = false;
        _throwStarted = false;

        // No scoreboard call here — next throw registers when ball is released again.
    }
}