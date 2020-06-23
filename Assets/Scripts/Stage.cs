using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class Stage : MonoBehaviour {
	public static Stage s;
	Vector3 clickMousePos;

	// Setting
	public float unitSpeed = 5f;
	public int cameraFollow = 0; // 0 == All, 1 == Friendly, 2 == None

	public int playerTeam;

	public int points;

	public Object[,] objects;
	public Object selectedObject;

	public Unit movedUnit;
	public bool enemyTurn;
	public bool unitMoving;
	public Unit killedUnit;

    void Start() {
    	Stage.s = this;
        generate(75);
    }

    public void generate(int size) {
    	// Destroy room objects
    	foreach (Transform child in transform.GetChild(0)) {
			GameObject.Destroy(child.gameObject);
		}
		points = 0;
		this.objects = new Object[size, size];
		selectedObject = null;

		movedUnit = null;
		enemyTurn = false;
		unitMoving = false;
		killedUnit = null;

		int[,] grid = new int[size, size];
		List<Room> rooms = new List<Room>();

		// Generate the starting room
		int width = 10;
		int height = 10;
		int xPos = (size - width - 1) / 2;
		int yPos = 1;
		// Room tiles are labeled with the index of the room+1 and surrounding tiles are labeled with -1
		for(int i = xPos - 1; i < xPos + width + 1; i++) {
			for(int j = yPos - 1; j < yPos + height + 1; j++) {
				if(i >= xPos && i < xPos + width && j >= yPos && j < yPos + height)
					grid[i, j] = rooms.Count + 1;
				else 
					grid[i, j] = -1;
			}
		}
  		Room startingRoom = ((GameObject)Instantiate(Resources.Load("prefabs/Room"), transform.GetChild(0))).GetComponent<Room>();
		startingRoom.generate(width, height, xPos, yPos);
		rooms.Add(startingRoom);
		Camera.main.transform.position = new Vector3(xPos + (width-1) / 2f, yPos + (width-1) / 2f, -10);
		// Populate the room with starting units
        startingRoom.addKing(playerTeam);
        startingRoom.addBishop(playerTeam);
		startingRoom.addKnight(playerTeam);
		startingRoom.addPawn(playerTeam);
		startingRoom.addQueen(playerTeam);
		startingRoom.addRook(playerTeam);

		int tries = 0;
		while(tries < 1000) {
			width = Random.Range(5, 13);
			height = Random.Range(5, 13);
			xPos = Random.Range(1, size - width);
			yPos = Random.Range(1, size - height);

			bool marked = false;
			for(int i = xPos - 1; i < Mathf.Min(xPos + width + 1, size); i++) {
				for(int j = yPos - 1; j < Mathf.Min(yPos + height + 1, size); j++) {
					if(grid[i, j] != 0) {
						marked = true;
						break;
					}
				}	
			}
			if(!marked) {
				tries = 0;
				// Room tiles are labeled with the index of the room+1 and surrounding tiles are labeled with -1
				for(int i = xPos - 1; i < xPos + width + 1; i++) {
					for(int j = yPos - 1; j < yPos + height + 1; j++) {
						if(i >= xPos && i < xPos + width && j >= yPos && j < yPos + height)
							grid[i, j] = rooms.Count + 1;
						else 
							grid[i, j] = -1;
					}
				}
  		  		Room room = ((GameObject)Instantiate(Resources.Load("prefabs/Room"), transform.GetChild(0))).GetComponent<Room>();
        		room.generate(width, height, xPos, yPos);
        		rooms.Add(room);
			} else
				tries++;
		}

		// foreach(Room r in rooms)
		startingRoom.generateCoridoors(grid, rooms, 1);

		// GameObject referenceTile = (GameObject)Instantiate(Resources.Load("prefabs/Tile"));
		// for(int i = 0; i < grid.GetLength(0); i++) {
		// 	for(int j = 0; j < grid.GetLength(1); j++) {
		// 		GameObject tile = (GameObject)Instantiate(referenceTile, transform.GetChild(0));
		// 		tile.transform.localPosition = new Vector2(i, j);
		// 		tile.GetComponent<SpriteRenderer>().sortingOrder = grid[i, j];
		// 		tile.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Untitled");
		// 	}
		// }
    }
    void Update() {
    	if(!unitMoving) {
    		if(selectedObject != null && ((Unit)selectedObject).route.Count > 0){
    			((Unit)selectedObject).move(((Unit)selectedObject).route[0]);
    			((Unit)selectedObject).route.RemoveAt(0);
    		} else if(enemyTurn) {
    			movedUnit.room.enemyTurn();
    		} else
				checkClick();

    	} else if (!CameraController.moving) {
    		movedUnit.transform.position = Vector2.MoveTowards(movedUnit.transform.position, movedUnit.pos, unitSpeed * Time.deltaTime);
    		if(killedUnit != null && Vector2.Distance(new Vector2(movedUnit.transform.position.x, movedUnit.transform.position.y), movedUnit.pos) < 0.6875f) {
    			killedUnit.GetComponent<Explodable>().generateFragments((movedUnit.pos - new Vector2(movedUnit.transform.position.x, movedUnit.transform.position.y)).normalized * unitSpeed);
    			killedUnit = null;
				GameObject.Find("ScoreText").gameObject.GetComponent<UnityEngine.UI.Text>().text = points + " Points";
    		} else if(movedUnit.transform.position.x == movedUnit.pos.x && movedUnit.transform.position.y == movedUnit.pos.y) {
    			unitMoving = false;
    			if(!enemyTurn && selectedObject != null && ((Unit)selectedObject).route.Count == 0)
					((Unit)selectedObject).markNextTiles();
    		}
    	}

    	if(Input.GetKeyDown("r"))
    		generate(75);
    }

    private void checkClick() {
    	if(Input.GetMouseButtonDown(0))
    		clickMousePos = Input.mousePosition;
        if(Input.GetMouseButtonUp(0) && !Stage.IsPointerOverUIObject() && clickMousePos == Input.mousePosition) {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            
    		removeMarkers();
            if(hit.transform != null) {
            	// Debug.Log(hit.transform.name);
            	if(hit.transform.GetComponent<Object>() is Clickable)
        			(hit.transform.GetComponent<Object>() as Clickable).onClick();
            } else
        		selectedObject = null;
        }
    }

    private void removeMarkers() {
    	foreach (Transform child in transform.GetChild(0)) {
    		foreach (Transform childchild in child.transform.GetChild(3)) {
				GameObject.Destroy(childchild.gameObject);
			}
		}
    }

    public static bool IsPointerOverUIObject() {
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}
}
