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
				case 0: addKing(1); break;
		        case 1: addBishop(1); break;
				case 2: addKnight(1); break;
				case 3: addPawn(1); break;
				case 4: addQueen(1); break;
				case 5: addRook(1); break;
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

    public void enemyTurn() {
    	List<Move> moves = new List<Move>();
    	foreach(Unit unit in enemyUnits) {
    		foreach(Vector2Int pos in unit.getNextTiles()) {
    			moves.Add(new Move(unit, pos));
    		}
    	}
    	if(moves.Count > 0) {
    		Move m = moves[Random.Range(0, moves.Count)];
    		if(Settings.cameraFollow == 0)
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
    	foreach(Transform child in GameObject.Find("Controller").transform) {
    		Room room = child.GetComponent<Room>();
    		if(room.inRoom(pos))
				return room;
    	}
    	return null;
    }

    public bool canMoveTo(Vector2Int pos, int team) {
    	if(inRoom(pos))
	    	return !(Controller.objects[pos.x, pos.y] is NotWalkable) &&
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
    public void addBishop(int team) {
    	Vector2Int pos = randomPos();
    	if(pos.x > -1) {
    		Bishop bishop = ((GameObject)Instantiate(Resources.Load("prefabs/Units/Bishop"), transform.GetChild(2))).GetComponent<Bishop>();
			bishop.setSprite(team);
			bishop.place(this, pos.x, pos.y);
			if(team != 0)
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
			if(team != 0)
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
			if(team != 0)
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
			if(team != 0)
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
			if(team != 0)
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
			if(team != 0)
				enemyUnits.Add(rook);
			else
				friendlyUnits.Add(rook);
		}
    }
}
