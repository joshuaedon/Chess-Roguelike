using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : Object, Clickable {
	public List<Vector2Int> route;

	public void onClick() {
		if(route.Count == 0)
			((Unit)Stage.s.selectedObject).move(this.pos);
		else
			((Unit)Stage.s.selectedObject).route = this.route;
	}
}
