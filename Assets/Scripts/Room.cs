using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
    
    public Vector2Int pos;
    public Vector2Int size;
    public int distFromStart;
    public List<Room> adjacentRooms;
	public List<Unit> enemyUnits;
	public List<Unit> friendlyUnits;
	int turnExited;

    public void generate(int width, int height, int xPos, int yPos, int enemies = -1, int holeSize = -1, float obstacleChance = 0.05f) {
  		this.pos = new Vector2Int(xPos, yPos);
  		this.size = new Vector2Int(width, height);
  		transform.position = new Vector2(xPos, yPos);
		int[,] pattern = generatePattern(width, height, holeSize);

		GameObject referenceTile = (GameObject)Instantiate(Resources.Load("prefabs/Tile"));
		GameObject referenceObstacle = (GameObject)Instantiate(Resources.Load("prefabs/Obstacle"));
    	Sprite[] tileSprites = Resources.LoadAll<Sprite>("Sprites/Tiles/DefaultTiles");
		for(int i = 0; i < width; i++) {
			for(int j = 0; j < height; j++) {
				GameObject tile = (GameObject)Instantiate(referenceTile, transform.GetChild(0));;
				if((i + j + pos.x + pos.y) % 2 == 0)
					tile.GetComponent<SpriteRenderer>().sprite = tileSprites[Random.Range(0, (int)tileSprites.Length/2)];
				else
					tile.GetComponent<SpriteRenderer>().sprite = tileSprites[Random.Range((int)tileSprites.Length/2, tileSprites.Length)];
				tile.transform.localPosition = new Vector2(i, j);

				if(pattern[i, j] == -1) {
					Obstacle obstacle = ((GameObject)Instantiate(referenceObstacle, transform.GetChild(1))).GetComponent<Obstacle>();
					obstacle.setSprite(pattern, i, j);
					obstacle.place(this, xPos + i, yPos + j);
				} else if(pattern[i, j] == 0 && Random.Range(0f, 1f) < obstacleChance) {
					Obstacle obstacle = ((GameObject)Instantiate(referenceObstacle, transform.GetChild(1))).GetComponent<Obstacle>();
					obstacle.setSprite();
					obstacle.place(this, xPos + i, yPos + j);
				}
			}
		}
    	Destroy(referenceTile);
    	Destroy(referenceObstacle);

    	if(enemies == -1)
    		enemies = Random.Range(3, 15);
    	for(int i = 0; i < enemies; i++) {
			switch(Random.Range(0, 6)) {
				case 0: addKing(1 - Stage.s.playerTeam); break;
		        case 1: addBishop(1 - Stage.s.playerTeam); break;
				case 2: addKnight(1 - Stage.s.playerTeam); break;
				case 3: addPawn(1 - Stage.s.playerTeam); break;
				case 4: addQueen(1 - Stage.s.playerTeam); break;
				case 5: addRook(1 - Stage.s.playerTeam); break;
			}
		}
    }
    private int[,] generatePattern(int width, int height, int holeSize) {
    	if(holeSize == -1)
    		holeSize = Random.Range(0, Mathf.Min(width/2, height/2));

    	int[,] grid = new int[width, height];
    	List<Vector2Int> holes = new List<Vector2Int>();

    	// Create holes
    	while(holeSize > 0) {
	    	List<Vector2Int> remainingTiles = new List<Vector2Int>();
	    	for(int i = 0; i < width - holeSize + 1; i++) {
	    		for(int j = 0; j < height - holeSize + 1; j++)
	    			remainingTiles.Add(new Vector2Int(i, j));
	    	}
	    	while(remainingTiles.Count > 0) {
	    		int index = Random.Range(0, remainingTiles.Count);
	    		Vector2Int selected = remainingTiles[index];
	    		remainingTiles.RemoveAt(index);

	    		bool marked = false;
	    		for(int i = selected.x; i < Mathf.Min(selected.x + holeSize + 1, width); i++) {
	    			for(int j = selected.y; j < Mathf.Min(selected.y + holeSize + 1, height); j++) {
	    				if(grid[i, j] != 0) {
	    					marked = true;
	    					break;
	    				}
	    			}
				}
	    		if(!marked) {
		    		for(int i = selected.x; i < Mathf.Min(selected.x + holeSize + 1, width); i++) {
		    			for(int j = selected.y; j < Mathf.Min(selected.y + holeSize + 1, height); j++) {
		    				if(i < selected.x + holeSize && j < selected.y + holeSize) {
		    					grid[i, j] = -1;
		    					holes.Add(new Vector2Int(i, j));
		    				} else
		    					grid[i, j] = 1;
		    			}
					}
				}
	    	}
	    	holeSize--;
	    }
	    // Randomely fill in some hole edge tiles
	    for(int i = 0; i < 6; i++) {
		    for(int j = 0; j < holes.Count; j++) {
		    	Vector2Int hole = holes[j];
		    	int sides = 0;
		    	if(hole.x - 1 >= 0 && grid[hole.x - 1, hole.y] != -1)
		    		sides++;
		    	if(hole.x + 1 < width && grid[hole.x + 1, hole.y] != -1)
		    		sides++;
		    	if(hole.y - 1 >= 0 && grid[hole.x, hole.y - 1] != -1)
		    		sides++;
		    	if(hole.y + 1 < height && grid[hole.x, hole.y + 1] != -1)
		    		sides++;
		    	
		    	if(Random.Range(0f, 1f) < Mathf.Min(2, sides - 1) / 4f) {
		    		grid[hole.x, hole.y] = 0;
		    		holes.RemoveAt(j);
		    	}
		    }
		}

		return grid;
    }

    public void generateCoridoors(int[,] grid, List<Room> rooms, int dist) {
    	this.distFromStart = dist;
    	generateNorthCoridoor(grid, rooms, 2);
		generateEastCoridoor(grid, rooms, 2);
		generateSouthCoridoor(grid, rooms, 2);
		generateWestCoridoor(grid, rooms, 2);			
    }
    private void generateNorthCoridoor(int[,] grid, List<Room> rooms, int thickness) {
    	int[] northRooms = new int[this.size.x];
		int minDist = -1;
		int minDistIndex = -1;
		int minDistTickness = -1;
		for(int i = 0; i < this.size.x; i++) {
			for(int j = 0; true; j++) {
				if(this.pos.y + this.size.y + j >= grid.GetLength(1) || grid[this.pos.x + i, this.pos.y + this.size.y + j] == -2 ||
					(grid[this.pos.x + i, this.pos.y + this.size.y + j] == -1 && grid[this.pos.x + i, this.pos.y + this.size.y + j-1] == -1 && grid[this.pos.x + i, this.pos.y + this.size.y + j-2] == -1)) {
					// Reached scene edge or another coridoor or next to a room (3 -1s in a row)
					northRooms[i] = -1;
					break;
				} else if(grid[this.pos.x + i, this.pos.y + this.size.y + j] > 0) {
					// Record reached room & if its the closest room, record the coridoor information
					northRooms[i] = grid[this.pos.x + i, this.pos.y + this.size.y + j];
					if(minDist == -1 || j < minDist) {
						minDist = j;
						minDistIndex = i;
						minDistTickness = 1;
					} else if(northRooms[minDistIndex] == northRooms[i] && i == minDistIndex + minDistTickness)
						minDistTickness++;
					break;
				}
			}
		}
		if(minDist != -1 && (rooms[northRooms[minDistIndex] - 1].distFromStart == 0 || Random.Range(0f, 1f) < 0.25f)) {
			// Return if there is already a coridoor connecting this room to the closest room
			foreach(Room adjacentRoom in adjacentRooms) {
				foreach(Room secondAdjacentRoom in adjacentRoom.adjacentRooms)
					if(secondAdjacentRoom == rooms[northRooms[minDistIndex] - 1])
						return;
			}
			// Reduce coridoor to thickness
			while(minDistTickness > thickness) {
				if(Random.Range(0f, 1f) < 0.5f)
					minDistIndex++;
				minDistTickness--;
			}
			Room coridoor = ((GameObject)Instantiate(Resources.Load("prefabs/Room"), transform.parent)).GetComponent<Room>();
			coridoor.generate(minDistTickness, minDist, this.pos.x + minDistIndex, this.pos.y + this.size.y, 0, 0, 0);
			coridoor.adjacentRooms.Add(this);
			coridoor.adjacentRooms.Add(rooms[northRooms[minDistIndex] - 1]);
			this.adjacentRooms.Add(coridoor);
			rooms[northRooms[minDistIndex] - 1].adjacentRooms.Add(coridoor);
			// Mark coridoor with -2
			for(int i = Mathf.Max(0, coridoor.pos.x - 1); i < Mathf.Min(coridoor.pos.x + coridoor.size.x + 1, grid.GetLength(0)); i++) {
				for(int j = coridoor.pos.y; j < coridoor.pos.y + coridoor.size.y; j++) {
					grid[i, j] = -2;
				}
			}
			if(rooms[northRooms[minDistIndex] - 1].distFromStart == 0)
				rooms[northRooms[minDistIndex] - 1].generateCoridoors(grid, rooms, this.distFromStart+1);
		}
    }
    private void generateEastCoridoor(int[,] grid, List<Room> rooms, int thickness) {
    	int[] eastRooms = new int[this.size.y];
		int minDist = -1;
		int minDistIndex = -1;
		int minDistTickness = -1;
		for(int i = 0; i < this.size.y; i++) {
			for(int j = 0; true; j++) {
				if(this.pos.x + this.size.x + j >= grid.GetLength(0) || grid[this.pos.x + this.size.x + j, this.pos.y + i] == -2 ||
					(grid[this.pos.x + this.size.x + j, this.pos.y + i] == -1 && grid[this.pos.x + this.size.x + j-1, this.pos.y + i] == -1 && grid[this.pos.x + this.size.x + j-2, this.pos.y + i] == -1)) {
					eastRooms[i] = -1;
					break;
				} else if(grid[this.pos.x + this.size.x + j, this.pos.y + i] > 0) {
					eastRooms[i] = grid[this.pos.x + this.size.x + j, this.pos.y + i];
					if(minDist == -1 || j < minDist) {
						minDist = j;
						minDistIndex = i;
						minDistTickness = 1;
					} else if(eastRooms[minDistIndex] == eastRooms[i] && i == minDistIndex + minDistTickness)
						minDistTickness++;
					break;
				}
			}
		}
		if(minDist != -1 && (rooms[eastRooms[minDistIndex] - 1].distFromStart == 0 || Random.Range(0f, 1f) < 0.25f)) {
			foreach(Room adjacentRoom in adjacentRooms) {
				foreach(Room secondAdjacentRoom in adjacentRoom.adjacentRooms)
					if(secondAdjacentRoom == rooms[eastRooms[minDistIndex] - 1])
						return;
			}
			while(minDistTickness > thickness) {
				if(Random.Range(0f, 1f) < 0.5f)
					minDistIndex++;
				minDistTickness--;
			}
			Room coridoor = ((GameObject)Instantiate(Resources.Load("prefabs/Room"), transform.parent)).GetComponent<Room>();
			coridoor.generate(minDist, minDistTickness, this.pos.x + this.size.x, this.pos.y + minDistIndex, 0, 0, 0);
			coridoor.adjacentRooms.Add(this);
			coridoor.adjacentRooms.Add(rooms[eastRooms[minDistIndex] - 1]);
			this.adjacentRooms.Add(coridoor);
			rooms[eastRooms[minDistIndex] - 1].adjacentRooms.Add(coridoor);
			for(int i = coridoor.pos.x; i < coridoor.pos.x + coridoor.size.x; i++) {
				for(int j = Mathf.Max(0, coridoor.pos.y - 1); j < Mathf.Min(coridoor.pos.y + coridoor.size.y + 1, grid.GetLength(1)); j++) {
					grid[i, j] = -2;
				}
			}
			if(rooms[eastRooms[minDistIndex] - 1].distFromStart == 0)
				rooms[eastRooms[minDistIndex] - 1].generateCoridoors(grid, rooms, this.distFromStart+1);
		}
    }
    private void generateSouthCoridoor(int[,] grid, List<Room> rooms, int thickness) {
    	int[] southRooms = new int[this.size.x];
		int minDist = -1;
		int minDistIndex = -1;
		int minDistTickness = -1;
		for(int i = 0; i < this.size.x; i++) {
			for(int j = 0; true; j++) {
				if(this.pos.y - j-1 < 0 || grid[this.pos.x + i, this.pos.y - j-1] == -2 ||
					(grid[this.pos.x + i, this.pos.y - j-1] == -1 && grid[this.pos.x + i, this.pos.y - j] == -1 && grid[this.pos.x + i, this.pos.y - j+1] == -1)) {
					southRooms[i] = -1;
					break;
				} else if(grid[this.pos.x + i, this.pos.y - j-1] > 0) {
					southRooms[i] = grid[this.pos.x + i, this.pos.y - j-1];
					if(minDist == -1 || j < minDist) {
						minDist = j;
						minDistIndex = i;
						minDistTickness = 1;
					} else if(southRooms[minDistIndex] == southRooms[i] && i == minDistIndex + minDistTickness)
						minDistTickness++;
					break;
				}
			}
		}
		if(minDist != -1 && (rooms[southRooms[minDistIndex] - 1].distFromStart == 0 || Random.Range(0f, 1f) < 0.25f)) {
			foreach(Room adjacentRoom in adjacentRooms) {
				foreach(Room secondAdjacentRoom in adjacentRoom.adjacentRooms)
					if(secondAdjacentRoom == rooms[southRooms[minDistIndex] - 1])
						return;
			}
			while(minDistTickness > thickness) {
				if(Random.Range(0f, 1f) < 0.5f)
					minDistIndex++;
				minDistTickness--;
			}
			Room coridoor = ((GameObject)Instantiate(Resources.Load("prefabs/Room"), transform.parent)).GetComponent<Room>();
			coridoor.generate(minDistTickness, minDist, this.pos.x + minDistIndex, this.pos.y - minDist, 0, 0, 0);
			coridoor.adjacentRooms.Add(this);
			coridoor.adjacentRooms.Add(rooms[southRooms[minDistIndex] - 1]);
			this.adjacentRooms.Add(coridoor);
			rooms[southRooms[minDistIndex] - 1].adjacentRooms.Add(coridoor);
			for(int i = Mathf.Max(0, coridoor.pos.x - 1); i < Mathf.Min(coridoor.pos.x + coridoor.size.x + 1, grid.GetLength(0)); i++) {
				for(int j = coridoor.pos.y; j < coridoor.pos.y + coridoor.size.y; j++) {
					grid[i, j] = -2;
				}
			}
			if(rooms[southRooms[minDistIndex] - 1].distFromStart == 0)
				rooms[southRooms[minDistIndex] - 1].generateCoridoors(grid, rooms, this.distFromStart+1);
		}
    }
    private void generateWestCoridoor(int[,] grid, List<Room> rooms, int thickness) {
    	int[] westRooms = new int[this.size.y];
		int minDist = -1;
		int minDistIndex = -1;
		int minDistTickness = -1;
		for(int i = 0; i < this.size.y; i++) {
			for(int j = 0; true; j++) {
				if(this.pos.x - j-1 < 0 || grid[this.pos.x - j-1, this.pos.y + i] == -2 ||
					(grid[this.pos.x - j-1, this.pos.y + i] == -1 && grid[this.pos.x - j, this.pos.y + i] == -1 && grid[this.pos.x - j+1, this.pos.y + i] == -1)) {
					westRooms[i] = -1;
					break;
				} else if(grid[this.pos.x - j-1, this.pos.y + i] > 0) {
					westRooms[i] = grid[this.pos.x - j-1, this.pos.y + i];
					if(minDist == -1 || j < minDist) {
						minDist = j;
						minDistIndex = i;
						minDistTickness = 1;
					} else if(westRooms[minDistIndex] == westRooms[i] && i == minDistIndex + minDistTickness)
						minDistTickness++;
					break;
				}
			}
		}
		if(minDist != -1 && (rooms[westRooms[minDistIndex] - 1].distFromStart == 0 || Random.Range(0f, 1f) < 0.25f)) {
			foreach(Room adjacentRoom in adjacentRooms) {
				foreach(Room secondAdjacentRoom in adjacentRoom.adjacentRooms)
					if(secondAdjacentRoom == rooms[westRooms[minDistIndex] - 1])
						return;
			}
			while(minDistTickness > thickness) {
				if(Random.Range(0f, 1f) < 0.5f)
					minDistIndex++;
				minDistTickness--;
			}
			Room coridoor = ((GameObject)Instantiate(Resources.Load("prefabs/Room"), transform.parent)).GetComponent<Room>();
			coridoor.generate(minDist, minDistTickness, this.pos.x - minDist, this.pos.y + minDistIndex, 0, 0, 0);
			coridoor.adjacentRooms.Add(this);
			coridoor.adjacentRooms.Add(rooms[westRooms[minDistIndex] - 1]);
			this.adjacentRooms.Add(coridoor);
			rooms[westRooms[minDistIndex] - 1].adjacentRooms.Add(coridoor);
			for(int i = coridoor.pos.x; i < coridoor.pos.x + coridoor.size.x; i++) {
				for(int j = Mathf.Max(0, coridoor.pos.y - 1); j < Mathf.Min(coridoor.pos.y + coridoor.size.y + 1, grid.GetLength(1)); j++) {
					grid[i, j] = -2;
				}
			}
			if(rooms[westRooms[minDistIndex] - 1].distFromStart == 0)
				rooms[westRooms[minDistIndex] - 1].generateCoridoors(grid, rooms, this.distFromStart+1);
		}
    }   

    public void enemyTurn() {
    	List<Move> moves = new List<Move>();
    	foreach(Unit unit in enemyUnits) {
    		foreach(Vector2Int pos in unit.getNextTiles()) {
    			moves.Add(new Move(unit, pos));
    		}
    	}
    	if(moves.Count > 0) {
    		Move m = moves[Random.Range(0, moves.Count)];
    		if(Stage.s.cameraFollow == 0)
    			CameraController.follow(m.unit.gameObject, 0.5f);
    		m.unit.move(m.pos);
    	}
    }
    struct Move {
    	public Unit unit;
    	public Vector2Int pos;

    	public Move(Unit unit, Vector2Int pos) {
    		this.unit = unit;
    		this.pos = pos;
    	}
    }

    public bool inRoom(Vector2Int pos) {
    	return pos.x >= this.pos.x && pos.x < this.pos.x + this.size.x && pos.y >= this.pos.y && pos.y < this.pos.y + this.size.y;
    }
    public Room returnRoom(Vector2Int pos) {
    	if(inRoom(pos))
    		return this;
    	// foreach(Room room in adjacentRooms) {
    	foreach(Transform child in Stage.s.transform.GetChild(0)) {
    		Room room = child.GetComponent<Room>();
    		if(room.inRoom(pos))
				return room;
    	}
    	return null;
    }

    public bool canMoveTo(Vector2Int pos, int team) {
    	if(inRoom(pos))
	    	return !(Stage.s.objects[pos.x, pos.y] is NotWalkable) &&
	    		   !(Stage.s.objects[pos.x, pos.y] is Unit && (team == ((Unit)Stage.s.objects[pos.x, pos.y]).team || team == -1));
		else if(team == Stage.s.playerTeam) {
			// Only player units can move to other rooms
			Room room = returnRoom(pos);
			// Can move inside rooms with no enemies or two tiles into rooms with
			if(room != null && (/*room.enemyUnits.Count == 0 || */(pos.x >= this.pos.x - 2 && pos.x < this.pos.x + this.size.x + 2 && pos.y >= this.pos.y - 2 && pos.y < this.pos.y + this.size.y + 2)))
				return room.canMoveTo(pos, -1);

		}
		return false;
    }

    private Vector2Int randomPos() {
        int tries = 0, x, y;
        do {
            x = Random.Range(this.pos.x, this.pos.x + this.size.x);
            y = Random.Range(this.pos.y, this.pos.y + this.size.y);
            tries++;
        } while(Stage.s.objects[x, y] != null && tries < 100);
        if(tries < 100)
        	return new Vector2Int(x, y);
        else
        	return new Vector2Int(-1, -1);
    }
    public void addBishop(int team) {
    	Vector2Int pos = randomPos();
    	if(pos.x > -1) {
    		Bishop bishop = ((GameObject)Instantiate(Resources.Load("prefabs/Units/Bishop"), transform.GetChild(2))).GetComponent<Bishop>();
			bishop.setSprite(team);
			bishop.place(this, pos.x, pos.y);
			if(team != Stage.s.playerTeam)
				enemyUnits.Add(bishop);
			else
				friendlyUnits.Add(bishop);
		}
    }
    public void addKing(int team) {
    	Vector2Int pos = randomPos();
    	if(pos.x > -1) {
    		King king = ((GameObject)Instantiate(Resources.Load("prefabs/Units/King"), transform.GetChild(2))).GetComponent<King>();
			king.setSprite(team);
			king.place(this, pos.x, pos.y);
			if(team != Stage.s.playerTeam)
				enemyUnits.Add(king);
			else
				friendlyUnits.Add(king);
		}
    }
    public void addKnight(int team) {
    	Vector2Int pos = randomPos();
    	if(pos.x > -1) {
    		Knight knight = ((GameObject)Instantiate(Resources.Load("prefabs/Units/Knight"), transform.GetChild(2))).GetComponent<Knight>();
			knight.setSprite(team);
			knight.place(this, pos.x, pos.y);
			if(team != Stage.s.playerTeam)
				enemyUnits.Add(knight);
			else
				friendlyUnits.Add(knight);
		}
    }
    public void addPawn(int team) {
    	Vector2Int pos = randomPos();
    	if(pos.x > -1) {
    		Pawn pawn = ((GameObject)Instantiate(Resources.Load("prefabs/Units/Pawn"), transform.GetChild(2))).GetComponent<Pawn>();
			pawn.setSprite(team);
			pawn.place(this, pos.x, pos.y);
			if(team != Stage.s.playerTeam)
				enemyUnits.Add(pawn);
			else
				friendlyUnits.Add(pawn);
		}
    }
    public void addQueen(int team) {
    	Vector2Int pos = randomPos();
    	if(pos.x > -1) {
    		Queen queen = ((GameObject)Instantiate(Resources.Load("prefabs/Units/Queen"), transform.GetChild(2))).GetComponent<Queen>();
			queen.setSprite(team);
			queen.place(this, pos.x, pos.y);
			if(team != Stage.s.playerTeam)
				enemyUnits.Add(queen);
			else
				friendlyUnits.Add(queen);
		}
    }
    public void addRook(int team) {
    	Vector2Int pos = randomPos();
    	if(pos.x > -1) {
    		Rook rook = ((GameObject)Instantiate(Resources.Load("prefabs/Units/Rook"), transform.GetChild(2))).GetComponent<Rook>();
			rook.setSprite(team);
			rook.place(this, pos.x, pos.y);
			if(team != Stage.s.playerTeam)
				enemyUnits.Add(rook);
			else
				friendlyUnits.Add(rook);
		}
    }
}
