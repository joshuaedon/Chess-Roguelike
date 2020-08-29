using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Unit {
    public override List<Vector2Int> getNextTiles() {
    	Vector2Int[] moveOffsets = new Vector2Int[] {new Vector2Int(-1, 0), new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(1, 0)};
		Vector2Int[] attackOffsets = new Vector2Int[] {new Vector2Int(-1, -1), new Vector2Int(-1, 1),	new Vector2Int(1, -1), new Vector2Int(1, 1)};

		List<Vector2Int> tilePositions = new List<Vector2Int>();
		foreach(Vector2Int tileOffset in moveOffsets) {
			Vector2Int tilePos = this.pos + tileOffset;
			if(this.room.canMoveTo(tilePos, this.team) && !(Controller.objects[tilePos.x, tilePos.y] is Unit))
				tilePositions.Add(tilePos);
		}
		foreach(Vector2Int tileOffset in attackOffsets) {
			Vector2Int tilePos = this.pos + tileOffset;
			if(this.room.canMoveTo(tilePos, this.team) && Controller.objects[tilePos.x, tilePos.y] is Unit)
				tilePositions.Add(tilePos);
		}
		return tilePositions;
    }
}
