using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Object : MonoBehaviour {

	public Sprite[] sprites;
	public Room room;
	public Vector2Int pos;

    public void place(Room room, int xPos, int yPos) {
    	if(this.room != null)
    		Stage.s.objects[this.pos.x, this.pos.y] = null;
		Stage.s.objects[xPos, yPos] = this;
		setPosition(room, xPos, yPos);
    }

    public void setPosition(Room room, int xPos, int yPos) {
    	this.room = room;
    	this.pos = new Vector2Int(xPos, yPos);
		transform.position = new Vector3(xPos, yPos, transform.position.z);
    }
}

public interface Clickable {
    void onClick();
}
public interface NotWalkable {}