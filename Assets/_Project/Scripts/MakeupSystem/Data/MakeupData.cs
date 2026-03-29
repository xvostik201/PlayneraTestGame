using UnityEngine;

[CreateAssetMenu(menuName = "Makeup/Makeup Data")]
public class MakeupData : ScriptableObject
{
    public MakeupTool type;
    public int index = 0;
    public Sprite applySprite;       
}