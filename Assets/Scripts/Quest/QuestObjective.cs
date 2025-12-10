using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    [Tooltip("Loại động vật cần collect")]
    public AnimalType animalType;
    
    [Tooltip("Số lượng động vật cần collect")]
    public int requiredAmount;
}
