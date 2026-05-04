using UnityEngine;

public class CardVisual : MonoBehaviour
{
    // Material slot indices — adjust to match your prefab's material order
    // Inspect your card prefab's MeshRenderer in Unity to confirm the order
    private const int FACE_SLOT = 0;
    private const int BACK_SLOT = 1;

    [Header("Card Back Texture")]
    public Texture2D cardBackTexture; // Assign in Inspector or load from Resources

    private MeshRenderer meshRenderer;
    private Material[] materials;
    private BlackjackCard cardData;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        // Clone materials so we don't modify shared assets
        materials = meshRenderer.materials;
    }

    public void Setup(BlackjackCard card)
    {
        cardData = card;
        if (card.isFaceUp)
            ShowFace();
        else
            ShowBack();
    }

    public void FlipUp()
    {
        if (cardData == null) return;
        cardData.isFaceUp = true;
        ShowFace();
    }

    void ShowFace()
    {
        // Card textures should be in Resources/Cards/ named e.g. "ace_of_spades"
        string texName = cardData.GetSpriteName();
        Texture2D tex = Resources.Load<Texture2D>($"Cards/{texName}");

        if (tex != null)
        {
            materials[FACE_SLOT].mainTexture = tex;
            meshRenderer.materials = materials;
        }
        else
        {
            Debug.LogWarning($"Card texture not found: Resources/Cards/{texName}");
        }
    }

    void ShowBack()
    {
        if (cardBackTexture != null)
        {
            materials[BACK_SLOT].mainTexture = cardBackTexture;
            meshRenderer.materials = materials;
        }
    }
}
