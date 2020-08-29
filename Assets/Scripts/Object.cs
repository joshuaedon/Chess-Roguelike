using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Object : MonoBehaviour {

	public Sprite[] sprites;
	public Vector2Int pos;
	public Room room;

    public void place(Room room, int xPos, int yPos) {
    	if(this.room != null)
    		Controller.objects[this.pos.x, this.pos.y] = null;
		Controller.objects[xPos, yPos] = this;
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