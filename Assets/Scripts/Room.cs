using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
	public GameObject TilesContainer;
	public GameObject ObjectsContainer;
	public GameObject UnitsContainer;
	public GameObject MarkersContainer;

    public RoomType type;

    public Vector2Int pos;
    public Vector2Int size;
    // public int distFromStart;
    public List<Room> adjacentRooms;
	int turnExited;

    public void generate(int width, int height, int xPos, int yPos, int holeSize = -1, float obstacleChance = 0.05f) {
    	setRandomType();

  		this.pos = new Vector2Int(xPos, yPos);
  		this.size = new Vector2Int(width, height);
  		transform.position = new Vector2(xPos, yPos);
		int[,] pattern = generatePattern(width, height, holeSize);

		GameObject referenceTile = (GameObject)Instantiate(Resources.Load("prefabs/Tile"));
		GameObject referenceObstacle = (GameObject)Instantiate(Resources.Load("prefabs/Obstacle"));
		for(int i = 0; i < width; i++) {
			for(int j = 0; j < height; j++) {
				GameObject tile = (GameObject)Instantiate(referenceTile, this.TilesContainer.transform);
				if((i + j + pos.x + pos.y) % 2 == 0)
					tile.GetComponent<SpriteRenderer>().sprite = type.whiteTile;//tileSprites[Random.Range(0, (int)tileSprites.Length/2)];
				else
					tile.GetComponent<SpriteRenderer>().sprite = type.blackTile;//tileSprites[Random.Range((int)tileSprites.Length/2, tileSprites.Length)];
				tile.transform.localPosition = new Vector2((i + j) / 2f, (j - i - 1f) / 4f);

				/*if(pattern[i, j] == -1) {
					Obstacle obstacle = ((GameObject)Instantiate(referenceObstacle, this.ObjectsContainer.transform)).GetComponent<Obstacle>();
					obstacle.setSprite(pattern, i, j);
					obstacle.place(this, xPos + i, yPos + j);
				} else if(pattern[i, j] == 0 && Random.Range(0f, 1f) < obstacleChance) {
					Obstacle obstacle = ((GameObject)Instantiate(referenceObstacle, this.ObjectsContainer.transform)).GetComponent<Obstacle>();
					obstacle.setSprite();
					obstacle.place(this, xPos + i, yPos + j);
				}*/
			}
		}
    	Destroy(referenceTile);
    	Destroy(referenceObstacle);

    	addUnit((UnitType)Resources.Load("UnitTypes/King"), 1);
    	for(int i = 0; i < Random.Range(2, 14); i++) {
			switch(Random.Range(0, 5)) {
		        case 0: addUnit((UnitType)Resources.Load("UnitTypes/Bishop"), 1); break;
				case 1: addUnit((UnitType)Resources.Load("UnitTypes/Knight"), 1); break;
				case 2: addUnit((UnitType)Resources.Load("UnitTypes/Pawn"), 1); break;
				case 3: addUnit((UnitType)Resources.Load("UnitTypes/Queen"), 1); break;
				case 4: addUnit((UnitType)Resources.Load("UnitTypes/Rook"), 1); break;
			}
		}
    }

    private void setRandomType() {
    	float rand = Random.Range(0f, 1f);
    	if(rand < 0.4f)
    		this.type = (RoomType)Resources.Load("RoomTypes/Default");
    	else if(rand < 0.6f)
    		this.type = (RoomType)Resources.Load("RoomTypes/Grass");
    	else if(rand < 0.8f)
    		this.type = (RoomType)Resources.Load("RoomTypes/StoneBrick");
    	else
    		this.type = (RoomType)Resources.Load("RoomTypes/Wood");
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

    public void turn(int team) { // int itterations, float x [half of considerations will go based on x+(inx/considerations)*(1-2*x) < Random(0f, 1f)], int maxConsiderations
    	List<Move> moves = new List<Move>();
    	foreach(Unit unit in Controller.units) {
    		if(unit.team == team) {
	    		foreach(Vector2Int pos in unit.getNextTiles()) {
	    			moves.Add(new Move(unit, pos));
	    		}
    		}
    	}
    	if(moves.Count > 0) {
    		Move m = moves[Random.Range(0, moves.Count)];
    		if(Settings.Instance.cameraFollowType == 0)
    			CameraController.Instance.follow(m.unit, 0.5f);
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
    	foreach(Transform child in GameObject.Find("Controller").transform) {
    		Room room = child.GetComponent<Room>();
    		if(room.inRoom(pos))
				return room;
    	}
    	return null;
    }

    public bool canMoveTo(Vector2Int pos, int team) {
    	if(inRoom(pos))
	    	return /*!(Controller.objects[pos.x, pos.y] is NotWalkable) &&*/
	    		   !(Controller.objects[pos.x, pos.y] is Unit && (team == ((Unit)Controller.objects[pos.x, pos.y]).team || team == -1));
		else if(team == 0) {
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
        } while(Controller.objects[x, y] != null && tries < 100);
        if(tries < 100)
        	return new Vector2Int(x, y);
        else
        	return new Vector2Int(-1, -1);
    }
    public void addUnit(UnitType unit, int team) {
    	Vector2Int pos = randomPos();
    	if(pos.x > -1) {
    		King king = ((GameObject)Instantiate(Resources.Load("prefabs/Units/King"), this.UnitsContainer.transform)).GetComponent<King>();
			king.setSprite(team);
			king.place(this, pos.x, pos.y);
			Controller.units.Add(king);
		}
    }
}
