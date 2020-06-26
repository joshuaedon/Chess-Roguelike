using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : Object, Clickable {
	public List<Vector2Int> route;

	public void onClick() {

		if(route.Count == 0)
			Controller.selectedUnit.move(this.pos);
		else
			Controller.selectedUnit.route = this.route;
	}
}
