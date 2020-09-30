using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Controller : MonoBehaviour {
	Vector3 clickMousePos;

	private int level;
	public static int score;

	public static List<Unit> units; // Change to an array of lists with a primary index for each team?
	public static Object[,] objects;
	private List<Room> rooms;
	public static Unit selectedUnit;

	public static Unit movedUnit;
	public static bool enemyTurn;
	public static bool unitMoving;
	public static Unit killedUnit;

    void Start() {
    	this.level = 0;
    	score = 0;
    	generateFirstLevel();
    }

    private void generateFirstLevel() {
    	// Destroy room objects
    	foreach (Transform child in transform) {
			GameObject.Destroy(child.gameObject);
		}
		units = new List<Unit>();
		objects = new Object[15, 15];
		this.rooms = new List<Room>();
		selectedUnit = null;

		movedUnit = null;
		enemyTurn = false;
		unitMoving = false;
		killedUnit = null;

		// Generate the starting room
  		Room startingRoom = ((GameObject)Instantiate(Resources.Load("prefabs/Room"), transform)).GetComponent<Room>();
  		rooms.Add(startingRoom);
		startingRoom.generate(15, 15, 0, 0);
		CameraController.Instance.transform.position = new Vector3((15-1) / 2f, 0f, -10);
		CameraController.Instance.pos = new Vector3((15-1) / 2f, 0f, -10);
		// Populate the room with starting unit
        startingRoom.addUnit((UnitType)Resources.Load("Units/Queen"), 0);
    }

    void Update() {
    	if(!unitMoving) {
    		// Unit being moved has arrived at its destination
    		if(selectedUnit != null && selectedUnit.route.Count > 0){
    			// Unit is partly moved through its route
    			selectedUnit.move(selectedUnit.route[0]);
    			selectedUnit.route.RemoveAt(0);
    		} else if(enemyTurn) {
    			movedUnit.room.turn(1);
    		} else
				checkClick();

    	} else if(!CameraController.Instance.isFollowing()) {
    		// An enemy only takes its move once the camera has reached their unit
    		// Move unit to its next tile
    		movedUnit.transform.position = Vector2.MoveTowards(movedUnit.transform.position, movedUnit.pos, Settings.Instance.unitSpeed * Time.deltaTime);
    		// 
    		if(killedUnit != null && Vector2.Distance(new Vector2(movedUnit.transform.position.x, movedUnit.transform.position.y), movedUnit.pos) < 0.6875f) {
    			killedUnit.GetComponent<Explodable>().generateFragments((movedUnit.pos - new Vector2(movedUnit.transform.position.x, movedUnit.transform.position.y)).normalized * Settings.Instance.unitSpeed);
    			/*if(killedUnit is King) {

			    		foreach(Unit unit in this.room.friendlyUnits.Count - 1; i >= 0; i--) {
							if(this == this.room.friendlyUnits[i])
								this.room.friendlyUnits.RemoveAt(i);
						}
					} else {
			    		for(int i = this.room.enemyUnits.Count - 1; i >= 0; i--) {
							if(this == this.room.enemyUnits[i])
								this.room.enemyUnits.RemoveAt(i);
						}
						Controller.score += returnPoints();
					}
    			}*/
    			killedUnit = null;
				GameObject.Find("ScoreText").gameObject.GetComponent<UnityEngine.UI.Text>().text = "Score: " + score + "    Level: " + level;
    		} else if(movedUnit.transform.position.x == movedUnit.pos.x && movedUnit.transform.position.y == movedUnit.pos.y) {
    			unitMoving = false;
    			if(!enemyTurn && selectedUnit != null && selectedUnit.route.Count == 0)
					selectedUnit.markNextTiles();
    		}
    	}

    	if(Input.GetKeyDown("r"))
    		generateFirstLevel();
    }

    private void checkClick() {
    	if(Input.GetMouseButtonDown(0))
    		clickMousePos = Input.mousePosition;
        if(Input.GetMouseButtonUp(0) && !Controller.IsPointerOverUIObject() && clickMousePos == Input.mousePosition) {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            
    		removeMarkers();
            if(hit.transform != null) {
            	// Debug.Log(hit.transform.name);
            	if(hit.transform.GetComponent<Object>() is Clickable)
        			(hit.transform.GetComponent<Object>() as Clickable).onClick();
            } else
        		selectedUnit = null;
        }
    }

    private void removeMarkers() {
    	foreach (Transform child in transform) {
    		foreach (Transform childchild in child.transform.GetChild(3)) {
				GameObject.Destroy(childchild.gameObject);
			}
		}
    }

    public static bool areEnemies() {
    	foreach(Unit unit in units) {
    		if(unit.team > 0)
    			return true;
    	}
    	return false;
    }

    public static bool IsPointerOverUIObject() {
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}
}
