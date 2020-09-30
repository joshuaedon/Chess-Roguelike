using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Marker Type", menuName = "Marker Type")]
public class MarkerType : ScriptableObject {
    public Sprite defaultSprite;
    public Sprite hoverSprite;
}
