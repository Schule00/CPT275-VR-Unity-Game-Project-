using UnityEngine;
using System.Collections;

public class SoccerBall : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelayAfterHit = 3f;
    [SerializeField] private float maxTimeAwayFromOrigin = 15f;
    [SerializeField] private float originThreshold = 0.1f;

    [Header("Score")]
    public int winnings = 0;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Rigidbody rb;
    private float timeAwayFromOrigin = 0f;
    private bool isRespawning = false;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isRespawning) return;

        if (Vector3.Distance(transform.position, originalPosition) > originThreshold)
        {
            timeAwayFromOrigin += Time.deltaTime;
            if (timeAwayFromOrigin >= maxTimeAwayFromOrigin)
            {
                ResetBall();
            }
        }
        else
        {
            timeAwayFromOrigin = 0f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isRespawning) return;

        // Search up the hierarchy — finds Can.cs whether it's on
        // the trigger collider's GameObject OR any parent of it
        TrashBall_Can can = other.GetComponentInParent<TrashBall_Can>();
        if (can != null)
        {
            winnings += can.pointsModifier;
            Debug.Log($"Scored {can.pointsModifier}! Total winnings: {winnings}");
            StartCoroutine(RespawnAfterDelay(respawnDelayAfterHit));
        }
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        isRespawning = true;

        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        yield return new WaitForSeconds(delay);

        ResetBall();

        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = true;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = true;
        isRespawning = false;
    }

    private void ResetBall()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        timeAwayFromOrigin = 0f;
    }
}