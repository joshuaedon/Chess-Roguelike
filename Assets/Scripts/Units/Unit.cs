using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class Unit : Object, Clickable {

	public int team;
	int maxHealth;
	int health;

	public List<Vector2Int> route;

	void Start() {
		// Set health
		this.maxHealth = 100;
		this.health = maxHealth;
	}

	public void setSprite(int team) {
		this.team = team;
		GetComponent<SpriteRenderer>().sprite = sprites[this.team];
    }

    public void onClick() {
    	Debug.Log("Hi");
		Stage.s.selectedObject = this;
		if(Stage.s.cameraFollow <= 1)
			CameraController.follow(gameObject, 0.25f);
		if(this.team == Stage.s.playerTeam)
			markNextTiles();
	}

    public void markNextTiles() {
		List<Vector2Int> nextTiles = new List<Vector2Int>();
		List<List<Vector2Int>> routes = new List<List<Vector2Int>>();

    	if(this.room.enemyUnits.Count > 0)
    		nextTiles = getNextTiles();
    	else {
    		routes = getRoutes();
    		foreach(List<Vector2Int> route in routes)
    			nextTiles.Add(route[route.Count - 1]);
    	}

    	for(int i = 0; i < nextTiles.Count; i++) {
    		Vector2Int tilePos = nextTiles[i];
			Marker marker = ((GameObject)Instantiate(Resources.Load("prefabs/Marker"), this.room.transform.GetChild(3))).GetComponent<Marker>();
			if(this.room.inRoom(tilePos)) {
				if(Stage.s.objects[tilePos.x, tilePos.y] is Unit)
					marker.GetComponent<SpriteRenderer>().sprite = marker.sprites[2];
				else
					marker.GetComponent<SpriteRenderer>().sprite = marker.sprites[0];
			} else {
				Room room = this.room.returnRoom(tilePos);
				if(room.enemyUnits.Count == 0)
					marker.GetComponent<SpriteRenderer>().sprite = marker.sprites[0];
				else
					marker.GetComponent<SpriteRenderer>().sprite = marker.sprites[1];
			}
			marker.setPosition(this.room, tilePos.x, tilePos.y);
			if(routes.Count > 0)
				marker.route = routes[i];
    	}
    	// Set camera back to following the selected piece after an enemy piece has moved
    	if(Stage.s.cameraFollow <= 1)
    		CameraController.follow(gameObject, 0.25f);
    }
    private List<List<Vector2Int>> getRoutes() {
    	List<List<Vector2Int>> routes = new List<List<Vector2Int>>();
    	List<List<Vector2Int>> toSearch = new List<List<Vector2Int>>();

    	// Add routes to all of the positions one step away
    	foreach(Vector2Int pos in getNextTiles()) {
    		List<Vector2Int> route = new List<Vector2Int>();
    		route.Add(pos);
    		toSearch.Add(route);
    	}

		Room curRoom = this.room;
		Vector2Int curPos = this.pos;
		while(toSearch.Count > 0) {
			// Move first element of toSearch to routes
			routes.Add(toSearch[0]);
			toSearch.RemoveAt(0);
			// Get that element's last coordinate's room and the position in the room
			room = curRoom.returnRoom(routes[routes.Count-1][routes[routes.Count-1].Count-1]);
			setPosition(room, routes[routes.Count-1][routes[routes.Count-1].Count-1].x, routes[routes.Count-1][routes[routes.Count-1].Count-1].y);

			// If there are no enemies in the room, search the next moves
			if(room.enemyUnits.Count == 0) {
				foreach(Vector2Int nextTilePos in getNextTiles()) {
					// Check if the coordinate is already in routes or toSearch
					bool alreadyFound = false;
					foreach(List<Vector2Int> route in routes) {
						if(nextTilePos == route[route.Count - 1]) {
							alreadyFound = true;
							break;
						}
					}
					foreach(List<Vector2Int> route in toSearch) {
						if(nextTilePos == route[route.Count - 1]) {
							alreadyFound = true;
							break;
						}
					}
					// If not add the coordinate with its route to toSearch
					if(!alreadyFound) {
						List<Vector2Int> newRoute = new List<Vector2Int>();
						foreach(Vector2Int tilePos in routes[routes.Count-1])
							newRoute.Add(tilePos);
						newRoute.Add(nextTilePos);
						toSearch.Add(newRoute);
					}
				}
			}
		}
		setPosition(curRoom, curPos.x, curPos.y);

		return routes;
    }
    public abstract List<Vector2Int> getNextTiles();

    public void move(Vector2Int pos) {
    	Room room = this.room.returnRoom(pos);
    	if(!(Stage.s.objects[pos.x, pos.y] is Unit) || ((Unit)Stage.s.objects[pos.x, pos.y]).dealDamage(100)) {
    		// Debug.Log((this.team == 0 ? "White " : "Black ") + this.GetType() + " - " + this.pos.x + ", " + this.pos.y + " - " + posOut.x + ", " + posOut.y);
    		Vector2Int temp = this.pos;
			place(room, pos.x, pos.y);
			transform.position = new Vector3(temp.x, temp.y, transform.position.z);
			transform.SetParent(room.transform.GetChild(2));
			Stage.s.movedUnit = this;
			if(room.enemyUnits.Count > 0)
				Stage.s.enemyTurn = this.team == Stage.s.playerTeam;
			Stage.s.unitMoving = true;
    	}
    }

    public bool dealDamage(int damage) {
    	this.health -= damage;
    	if(checkDeath()) {
    		Stage.s.killedUnit = this;
    		if(this.team != Stage.s.playerTeam) {
	    		for(int i = this.room.enemyUnits.Count - 1; i >= 0; i--) {
					if(this == this.room.enemyUnits[i])
						this.room.enemyUnits.RemoveAt(i);
				}
				Stage.s.points += returnPoints();
			}
    	}
    	return checkDeath();
    }
    public bool checkDeath() {
    	return health <= 0;
    }
    public abstract int returnPoints();
}
