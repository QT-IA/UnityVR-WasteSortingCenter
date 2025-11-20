using UnityEngine;
public class WasteItem : MonoBehaviour
{
    [Tooltip("Type de déchet (pour logique future)")]
    public string wasteType = "Generic";

    [Tooltip("Si vrai, l'objet est triable (+1 point). Si faux, non triable (-1 point).")]
    public bool isAppropriate = true;

    [Tooltip("Points bonus/malus personnalisés (par défaut +1/-1)")]
    public int pointValue = 0; // 0 = utiliser la logique par défaut (+1/-1)

    // Méthodes utilitaires
    public int GetPoints()
    {
        if (pointValue != 0) return pointValue;
        return isAppropriate ? 1 : -1;
    }

    public override string ToString()
    {
        return $"{wasteType}: {(isAppropriate ? "Triable" : "Non triable")} ({GetPoints()} pts)";
    }
}