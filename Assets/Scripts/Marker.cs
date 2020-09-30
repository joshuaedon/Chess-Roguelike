using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : Object, Clickable {
	public Sprite sprite;

	public MarkerType type;
	public List<Vector2Int> route;

	public void onClick() {
		if(route.Count == 0)
			Controller.selectedUnit.move(this.pos);
		else
			Controller.selectedUnit.route = this.route;
	}

	void OnMouseEnter() {
		this.sprite = this.type.hoverSprite;
	}

	public void OnMouseExit() {
		this.sprite = this.type.defaultSprite;
	}
}
