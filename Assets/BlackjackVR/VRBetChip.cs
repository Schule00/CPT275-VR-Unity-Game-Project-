using UnityEngine;

// Attach this to physical chip GameObjects on the table.
// Requires a Collider on the chip and a trigger Collider on the bet zone.
// Tag your bet zone GameObject as "BetZone".

public class VRBetChip : MonoBehaviour
{
    [Header("Settings")]
    public int chipValue = 25; // Dollar value of this chip

    private BlackjackGameManager gameManager;
    private Vector3 originalPosition;
    private bool hasBeenPlayed = false;

    void Awake()
    {
        gameManager = FindObjectOfType<BlackjackGameManager>();
        originalPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenPlayed) return;

        if (other.CompareTag("BetZone"))
        {
            hasBeenPlayed = true;
            gameManager.PlaceBet(chipValue);

            // Snap chip to the center of the bet zone
            transform.position = other.transform.position;

            Debug.Log($"Bet placed: ${chipValue}");
        }
    }

    // Call this to return the chip to its original position (e.g. on round end)
    public void ResetChip()
    {
        hasBeenPlayed = false;
        transform.position = originalPosition;
    }
}
