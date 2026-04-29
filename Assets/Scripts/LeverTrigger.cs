using UnityEngine;

public class LeverTrigger : MonoBehaviour
{
    public HingeJoint hinge;
    private bool hasTriggered = false;

    void Update()
    {
        // Check if the lever is pushed past 85 degrees (near the 90 max)
        if (hinge.angle > 85f && !hasTriggered)
        {
            Debug.Log("JACKPOT! Lever Pulled.");
            hasTriggered = true;
            // Add your "Start Spinning" code here!
        }
        // Reset the trigger once the lever goes back up
        else if (hinge.angle < 10f)
        {
            hasTriggered = false;
        }
    }
}
