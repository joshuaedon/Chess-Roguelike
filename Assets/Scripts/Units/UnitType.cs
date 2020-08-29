using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Type", menuName = "Unit Type")]
public class UnitType : ScriptableObject {
    public new string name;

    public Sprite[] sprites;

    public int cost;
}
