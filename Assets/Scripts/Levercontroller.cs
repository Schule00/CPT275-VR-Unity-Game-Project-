using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attach to: Lever_Hinge (1) — the same object that has the HingeJoint.
///
/// Reads hinge.angle directly — no euler angle guessing, no Input, no XR events.
/// User confirmed hinge.angle reaches 177 at full pull.
/// Default triggerAngle = 160 to give a safe buffer.
/// </summary>
[RequireComponent(typeof(HingeJoint))]
public class LeverController : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("Fire when hinge.angle reaches this. Your hinge hits 177 so 160 is a safe trigger.")]
    public float triggerAngle = 160f;

    [Tooltip("Hinge angle must drop below this before it can fire again (your Min limit is 90).")]
    public float resetAngle = 100f;

    [Header("Event — wire to SlotMachineUI.TriggerSpin()")]
    public UnityEvent OnLeverMaxPulled;

    private HingeJoint hinge;
    private bool hasTriggered;

    void Start()
    {
        hinge = GetComponent<HingeJoint>();
        if (hinge == null)
            Debug.LogError("[Lever] No HingeJoint found on this GameObject!");
        else
            Debug.Log($"[Lever] Ready. hinge.angle will be logged every half second. " +
                      $"Fires when angle >= {triggerAngle}");
    }

    void Update()
    {
        if (hinge == null) return;

        float angle = hinge.angle;

        // Log every 30 frames (~twice per second) so you can see it moving
        if (Time.frameCount % 30 == 0)
            Debug.Log($"[Lever] hinge.angle = {angle:F1}  (fires at {triggerAngle})");

        // Reset so lever can fire again on the next pull
        if (hasTriggered && angle < resetAngle)
        {
            hasTriggered = false;
            Debug.Log("[Lever] Reset — ready for next pull.");
        }

        // Fire!
        if (!hasTriggered && angle >= triggerAngle)
        {
            hasTriggered = true;
            Debug.Log($"[Lever] *** FIRED at hinge.angle={angle:F1} ***");
            OnLeverMaxPulled?.Invoke();
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (hinge == null) hinge = GetComponent<HingeJoint>();
        if (hinge == null) return;
        Vector3 pos = transform.TransformPoint(hinge.anchor);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, 0.05f);
        UnityEditor.Handles.Label(pos + Vector3.up * 0.1f,
            Application.isPlaying
                ? $"hinge.angle = {hinge.angle:F1}  trigger = {triggerAngle}"
                : $"trigger = {triggerAngle}");
    }
#endif
}