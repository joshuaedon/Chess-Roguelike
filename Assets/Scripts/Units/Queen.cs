using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : Unit {
    public override List<Vector2Int> getNextTiles() {
		List<Vector2Int> tilePositions = new List<Vector2Int>();
		Vector2Int tilePos = new Vector2Int(this.pos.x + 1, this.pos.y);
		while(this.room.canMoveTo(tilePos, this.team)) {
			tilePositions.Add(tilePos);
			if(Controller.objects[tilePos.x, tilePos.y] is Unit)
				break;
			tilePos = new Vector2Int(tilePos.x + 1, tilePos.y);
		}
		tilePos = new Vector2Int(this.pos.x - 1, this.pos.y);
		while(this.room.canMoveTo(tilePos, this.team)) {
			tilePositions.Add(tilePos);
			if(Controller.objects[tilePos.x, tilePos.y] is Unit)
				break;
			tilePos = new Vector2Int(tilePos.x - 1, tilePos.y);
		}
		tilePos = new Vector2Int(this.pos.x, this.pos.y + 1);
		while(this.room.canMoveTo(tilePos, this.team)) {
			tilePositions.Add(tilePos);
			if(Controller.objects[tilePos.x, tilePos.y] is Unit)
				break;
			tilePos = new Vector2Int(tilePos.x, tilePos.y + 1);
		}
		tilePos = new Vector2Int(this.pos.x, this.pos.y - 1);
		while(this.room.canMoveTo(tilePos, this.team)) {
			tilePositions.Add(tilePos);
			if(Controller.objects[tilePos.x, tilePos.y] is Unit)
				break;
			tilePos = new Vector2Int(tilePos.x, tilePos.y - 1);
		}
		tilePos = new Vector2Int(this.pos.x + 1, this.pos.y + 1);
		while(this.room.canMoveTo(tilePos, this.team)) {
			tilePositions.Add(tilePos);
			if(Controller.objects[tilePos.x, tilePos.y] is Unit)
				break;
			tilePos = new Vector2Int(tilePos.x + 1, tilePos.y + 1);
		}
		tilePos = new Vector2Int(this.pos.x + 1, this.pos.y - 1);
		while(this.room.canMoveTo(tilePos, this.team)) {
			tilePositions.Add(tilePos);
			if(Controller.objects[tilePos.x, tilePos.y] is Unit)
				break;
			tilePos = new Vector2Int(tilePos.x + 1, tilePos.y - 1);
		}
		tilePos = new Vector2Int(this.pos.x - 1, this.pos.y + 1);
		while(this.room.canMoveTo(tilePos, this.team)) {
			tilePositions.Add(tilePos);
			if(Controller.objects[tilePos.x, tilePos.y] is Unit)
				break;
			tilePos = new Vector2Int(tilePos.x - 1, tilePos.y + 1);
		}
		tilePos = new Vector2Int(this.pos.x - 1, this.pos.y - 1);
		while(this.room.canMoveTo(tilePos, this.team)) {
			tilePositions.Add(tilePos);
			if(Controller.objects[tilePos.x, tilePos.y] is Unit)
				break;
			tilePos = new Vector2Int(tilePos.x - 1, tilePos.y - 1);
		}
		return tilePositions;
    }

    public override int returnPoints() {
    	return 9;
    }
}
