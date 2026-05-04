using System.Collections.Generic;
using UnityEngine;

public class CardLayoutManager : MonoBehaviour
{
    [Header("Card Prefab")]
    public GameObject cardPrefab; // Drag any card prefab here (e.g. Club_2)

    [Header("Layout Positions")]
    public Transform playerCardOrigin; // Where player cards start
    public Transform dealerCardOrigin; // Where dealer cards start
    public float cardSpacing = 0.08f;  // Meters between cards in VR

    private List<GameObject> playerCardObjects = new List<GameObject>();
    private List<GameObject> dealerCardObjects = new List<GameObject>();

    public void PlaceCard(BlackjackCard card, bool isPlayer)
    {
        GameObject cardObj = Instantiate(cardPrefab);

        // Add collider if not present
        if (cardObj.GetComponent<Collider>() == null)
        {
            BoxCollider col = cardObj.AddComponent<BoxCollider>();
            col.size = new Vector3(0.063f, 0.001f, 0.088f); // Standard card size in meters
        }

        // Apply card visual
        CardVisual visual = cardObj.GetComponent<CardVisual>();
        if (visual == null) visual = cardObj.AddComponent<CardVisual>();
        visual.Setup(card);

        if (isPlayer)
        {
            Vector3 pos = playerCardOrigin.position +
                          playerCardOrigin.right * (playerCardObjects.Count * cardSpacing);
            cardObj.transform.position = pos;
            cardObj.transform.rotation = playerCardOrigin.rotation;
            playerCardObjects.Add(cardObj);
        }
        else
        {
            Vector3 pos = dealerCardOrigin.position +
                          dealerCardOrigin.right * (dealerCardObjects.Count * cardSpacing);
            cardObj.transform.position = pos;
            cardObj.transform.rotation = dealerCardOrigin.rotation;
            dealerCardObjects.Add(cardObj);
        }
    }

    public void FlipAllDealerCards()
    {
        foreach (var obj in dealerCardObjects)
            obj.GetComponent<CardVisual>()?.FlipUp();
    }

    public void ClearTable()
    {
        foreach (var obj in playerCardObjects) Destroy(obj);
        foreach (var obj in dealerCardObjects) Destroy(obj);
        playerCardObjects.Clear();
        dealerCardObjects.Clear();
    }
}
