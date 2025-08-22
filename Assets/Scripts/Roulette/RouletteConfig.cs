using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="RouletteConfig", menuName = "Merge/RouletteConfig")]
public class RouletteConfig: ScriptableObject
{
    public List<WeightedRouletteItem> rouletteItems;
}