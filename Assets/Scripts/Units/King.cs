using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Unit {
    public override List<Vector2Int> getNextTiles() {
    	Vector2Int[] offsets = new Vector2Int[] {
			new Vector2Int(1, -1),  new Vector2Int(1, 0),  new Vector2Int(1, 1),
			new Vector2Int(0, -1),                         new Vector2Int(0, 1),
			new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1)
		};
		List<Vector2Int> tilePositions = new List<Vector2Int>();
		foreach(Vector2Int tileOffset in offsets) {
			Vector2Int tilePos = this.pos + tileOffset;
			if(this.room.canMoveTo(tilePos, this.team))
				tilePositions.Add(tilePos);
		}
		return tilePositions;
    }

    public override int returnPoints() {
    	return 2;
    }
}
