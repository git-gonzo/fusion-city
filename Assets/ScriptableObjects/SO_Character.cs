using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Fame/Character")]
public class SO_Character : ScriptableObject
{
    public int characterId;
    public Sprite characterPortrait;
    public Sprite characterImage;
    public string characterName;
}