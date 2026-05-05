using System.Collections;
using UnityEngine;

/// <summary>
/// Attach this script to EACH Cylinder (bowling pin) GameObject.
/// v3 — fixes double-counting by locking each pin to one score event per throw.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BowlingPin : MonoBehaviour
{
    [Header("Reset Settings")]
    [Tooltip("Seconds after a pin is disturbed before it resets.")]
    public float resetDelay = 7f;

    [Tooltip("How long (seconds) the pin lerps back to standing on reset.")]
    public float resetLerpDuration = 0.4f;

    [Header("Tilt Detection")]
    [Tooltip("Degrees from upright before the pin is considered knocked over.")]
    [Range(5f, 45f)]
    public float knockedAngleThreshold = 20f;

    // ── Private state ────────────────────────────────────────────────────────
    private Rigidbody _rb;

    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private bool _resetQueued = false;
    private bool _isResetting = false;

    // KEY FIX: once a pin scores in a throw, this blocks it from scoring again
    // until PrepareForNewThrow() is called by the scoreboard.
    private bool _countedThisThrow = false;

    private Coroutine _resetCoroutine = null;

    // ── Unity lifecycle ──────────────────────────────────────────────────────
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    private void Update()
    {
        if (_resetQueued || _isResetting)
            return;

        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);

        if (tiltAngle > knockedAngleThreshold)
            QueueReset();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_resetQueued || _isResetting)
            return;

        if (collision.gameObject.CompareTag("BowlingBall"))
            QueueReset();
    }

    // ── Reset logic ──────────────────────────────────────────────────────────
    private void QueueReset()
    {
        _resetQueued = true;

        // Only register with the scoreboard ONCE per throw per pin.
        if (!_countedThisThrow)
        {
            _countedThisThrow = true;

            if (BowlingScoreboard.Instance != null)
                BowlingScoreboard.Instance.RegisterPinDown();
        }

        if (_resetCoroutine != null)
            StopCoroutine(_resetCoroutine);

        _resetCoroutine = StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);

        _isResetting = true;
        yield return StartCoroutine(LerpToStart());
        _isResetting = false;
        _resetQueued = false;
        // NOTE: _countedThisThrow stays TRUE until PrepareForNewThrow() clears it.
        // This prevents a pin from scoring again after it stands back up in the
        // same throw.
    }

    private IEnumerator LerpToStart()
    {
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        float elapsed = 0f;
        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;

        while (elapsed < resetLerpDuration)
        {
            float t = elapsed / resetLerpDuration;
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(fromPos, _startPosition, t);
            transform.rotation = Quaternion.Slerp(fromRot, _startRotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = _startPosition;
        transform.rotation = _startRotation;

        _rb.isKinematic = false;
    }

    // ── Public API ───────────────────────────────────────────────────────────
    /// <summary>
    /// Called by BowlingScoreboard at the start of each new throw.
    /// Clears the per-throw score lock so this pin can be counted again.
    /// </summary>
    public void PrepareForNewThrow()
    {
        _countedThisThrow = false;
    }

    /// <summary>
    /// Instantly hard-resets this pin (e.g. new game button).
    /// </summary>
    public void ForceReset()
    {
        StopAllCoroutines();
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.position = _startPosition;
        transform.rotation = _startRotation;
        _rb.isKinematic = false;
        _resetQueued = false;
        _isResetting = false;
        _countedThisThrow = false;
    }
}