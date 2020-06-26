using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {
    public static float defaultPanSpeed = 2f;
    public static float zoomSpeed = 5f;

    public static GameObject following;
    public static float followLeniency = 0.25f;
    public static float followSpeed = 15f;
    public static bool moving;

    // Android support
    Vector3 touchPrev;
    int touchCountPrev; 

    void Update() {
        Vector3 pos = transform.position;

        float panSpeed = defaultPanSpeed * GetComponent<Camera>().orthographicSize;
        if(Input.GetKey("w")) {
            pos.y += panSpeed * Time.deltaTime;
			following = null;
        }
        if(Input.GetKey("a")) {
            pos.x -= panSpeed * Time.deltaTime;
			following = null;
        }
        if(Input.GetKey("s")) {
            pos.y -= panSpeed * Time.deltaTime;
			following = null;
        }
        if(Input.GetKey("d")) {
            pos.x += panSpeed * Time.deltaTime;
			following = null;
        }

       	// Android support
		if(Input.GetMouseButtonDown(0) && !Controller.IsPointerOverUIObject()) {
       		// Set initial position of touch
       		touchCountPrev = Input.touchCount;
			touchPrev = Input.mousePosition;
		}
		// Make sure position is different so clicking doesn't set following to null
		if(Input.GetMouseButton(0) && !Controller.IsPointerOverUIObject() && (touchPrev.x != Input.mousePosition.x) && (touchPrev.y != Input.mousePosition.y)) {
			if(touchCountPrev <= 1 && Input.touchCount <= 1)
				pos += new Vector3(touchPrev.x - Input.mousePosition.x, touchPrev.y - Input.mousePosition.y, 0) * GetComponent<Camera>().orthographicSize * 2 / Screen.height;
			// Update touch position so that the camera stops moving when the user's finger does
       		touchCountPrev = Input.touchCount;
			touchPrev = Input.mousePosition;
			// Unlock the camera from the selected unit
			following = null;
		}

		// Follow selected unit to the within a quarter of the screen in each direction
        if(following != null) {
        	Vector2 destination = new Vector2(Mathf.Clamp(pos.x, following.transform.position.x - followLeniency * GetComponent<Camera>().orthographicSize * Screen.width / Screen.height, following.transform.position.x + followLeniency * GetComponent<Camera>().orthographicSize * Screen.width / Screen.height),
        									  Mathf.Clamp(pos.y, following.transform.position.y - followLeniency * GetComponent<Camera>().orthographicSize, following.transform.position.y + followLeniency * GetComponent<Camera>().orthographicSize));
    		Vector2 v = Vector2.MoveTowards(pos, destination, followSpeed * Time.deltaTime);
    		if(Vector2.Distance(v, new Vector2(pos.x, pos.y)) > 1.1f * Settings.unitSpeed * Time.deltaTime)
    			moving = true;
			else
    			moving = false;
			pos = new Vector3(v.x, v.y, -10);
        } else
        	moving = false;


        // Zoom the camera in/out
        if(!Controller.IsPointerOverUIObject() && !Input.GetKey(KeyCode.LeftControl))
        	GetComponent<Camera>().orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * 100f * Time.deltaTime;
    	// Android support
		if(Input.touchCount == 2) {
			Vector2 touchZeroPrevPos = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition;
			Vector2 touchOnePrevPos  = Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition;

			float prevMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float curMag = (Input.GetTouch(0).position - Input.GetTouch(1).position).magnitude;

			GetComponent<Camera>().orthographicSize -= (curMag - prevMag) * zoomSpeed * Time.deltaTime;
		}
        // Cap the camera's size and position
		GetComponent<Camera>().orthographicSize = Mathf.Clamp(GetComponent<Camera>().orthographicSize, 1f, 30f);
		// Vector2 currentRoomPosition = Stage.currentRoom.transform.position;
        // pos.x = Mathf.Clamp(pos.x, currentRoomPosition.x - Stage.currentRoom.objects.GetLength(0) / 2f, currentRoomPosition.x + Stage.currentRoom.objects.GetLength(0) / 2f);
        // pos.y = Mathf.Clamp(pos.y, currentRoomPosition.y - Stage.currentRoom.objects.GetLength(1) / 2f, currentRoomPosition.y + Stage.currentRoom.objects.GetLength(1) / 2f);
        
        transform.position = pos;
    }

    public static void follow(GameObject following, float followLeniency) {
    	CameraController.following = following;
		CameraController.followLeniency = followLeniency;
    }
}