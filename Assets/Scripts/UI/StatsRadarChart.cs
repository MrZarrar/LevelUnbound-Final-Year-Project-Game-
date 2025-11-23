using UnityEngine;
using UnityEngine.UI;

public class StatsRadarChart : MaskableGraphic
{
    [SerializeField] private PlayerStats playerStats;

    [SerializeField] private float maxStatValue = 50f;
    [SerializeField] private float graphSize = 100f;   

    private float currentStr, currentAgi, currentInt, currentVit;

    void Update()
    {
        if (playerStats == null) return;

        float lerpSpeed = 10f * Time.unscaledDeltaTime; 
        
        currentStr = Mathf.Lerp(currentStr, playerStats.strength.GetValue(), lerpSpeed);
        currentAgi = Mathf.Lerp(currentAgi, playerStats.agility.GetValue(), lerpSpeed);
        currentInt = Mathf.Lerp(currentInt, playerStats.intelligence.GetValue(), lerpSpeed);
        currentVit = Mathf.Lerp(currentVit, playerStats.vitality.GetValue(), lerpSpeed);

        SetVerticesDirty(); 
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float nStr = Mathf.Clamp01(currentStr / maxStatValue);
        float nAgi = Mathf.Clamp01(currentAgi / maxStatValue);
        float nInt = Mathf.Clamp01(currentInt / maxStatValue);
        float nVit = Mathf.Clamp01(currentVit / maxStatValue);


        Vector2 center = Vector2.zero;

        // Top (Strength)
        Vector2 pTop = new Vector2(0, nStr * graphSize);
        
        // Right (Agility)
        Vector2 pRight = new Vector2(nAgi * graphSize, 0);
        
        // Bottom (Intelligence) - Note negative Y
        Vector2 pBottom = new Vector2(0, -nInt * graphSize);
        
        // Left (Vitality) - Note negative X
        Vector2 pLeft = new Vector2(-nVit * graphSize, 0);

        vh.AddVert(center, color, Vector2.zero); // Index 0
        vh.AddVert(pTop, color, Vector2.zero);    // Index 1
        vh.AddVert(pRight, color, Vector2.zero);  // Index 2
        vh.AddVert(pBottom, color, Vector2.zero); // Index 3
        vh.AddVert(pLeft, color, Vector2.zero);   // Index 4

        // Center -> Top -> Right
        vh.AddTriangle(0, 1, 2);
        // Center -> Right -> Bottom
        vh.AddTriangle(0, 2, 3);
        // Center -> Bottom -> Left
        vh.AddTriangle(0, 3, 4);
        // Center -> Left -> Top
        vh.AddTriangle(0, 4, 1);
    }
}