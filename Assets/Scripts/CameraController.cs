using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {
    public static CameraController _instance;

    private Object followObject = null;

    private float orthSize = 10f;
    public Vector3 pos = new Vector3(0f, 0f, -10f);

    // public static float followSpeed = 15f;
    // public static bool moving;

    // Android support
    Vector3 touchPrev;
    int touchCountPrev;

    void Update() {
        // Follow object
        if(this.followObject != null)
            this.pos = new Vector3(this.followObject.transform.position.x, this.followObject.transform.position.y, -10f);

        // Zoom
        if(Input.GetAxis("Mouse ScrollWheel") != 0)
            this.orthSize = Mathf.Clamp(this.orthSize - Input.GetAxis("Mouse ScrollWheel") * Settings.Instance.zoomSpeed * Time.deltaTime, Settings.Instance.minZoom, Settings.Instance.maxZoom);

        // // Android support
        if(Input.touchCount == 2) {
            Vector2 touchZeroPrevPos = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition;
            Vector2 touchOnePrevPos  = Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition;

            float prevMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float curMag = (Input.GetTouch(0).position - Input.GetTouch(1).position).magnitude;

            this.orthSize = Mathf.Clamp(this.orthSize - (curMag - prevMag) * Settings.Instance.zoomSpeed * Time.deltaTime, Settings.Instance.minZoom, Settings.Instance.maxZoom);
        }

        // Pan
        if(Input.GetKey(KeyCode.D)) {
            this.pos.x += Settings.Instance.panSpeed * Time.deltaTime; unfollow(); }
        if(Input.GetKey(KeyCode.A)) {
            this.pos.x -= Settings.Instance.panSpeed * Time.deltaTime; unfollow(); }
        if(Input.GetKey(KeyCode.W)) {
            this.pos.y += Settings.Instance.panSpeed * Time.deltaTime / 2f; unfollow(); }
        if(Input.GetKey(KeyCode.S)) {
            this.pos.y -= Settings.Instance.panSpeed * Time.deltaTime / 2f; unfollow(); }

        // // Android support
        if(Input.GetMouseButtonDown(0) && !Controller.IsPointerOverUIObject()) {
            // Set initial position of touch
            touchCountPrev = Input.touchCount;
            touchPrev = Input.mousePosition;
        }
        // // Make sure position is different so dragging doesn't set following to null
        if(Input.GetMouseButton(0) && !Controller.IsPointerOverUIObject() && (touchPrev.x != Input.mousePosition.x) && (touchPrev.y != Input.mousePosition.y)) {
            if(touchCountPrev <= 1 && Input.touchCount <= 1)
                pos += new Vector3(touchPrev.x - Input.mousePosition.x, touchPrev.y - Input.mousePosition.y, 0) * GetComponent<Camera>().orthographicSize * 2 / Screen.height;
            // Update touch position so that the camera stops moving when the user's finger does
            touchCountPrev = Input.touchCount;
            touchPrev = Input.mousePosition;
            // Unlock the camera from the selected unit
            unfollow();
        }
        // pos.x = Mathf.Clamp(pos.x, currentRoomPosition.x - Stage.currentRoom.objects.GetLength(0) / 2f, currentRoomPosition.x + Stage.currentRoom.objects.GetLength(0) / 2f);
        // pos.y = Mathf.Clamp(pos.y, currentRoomPosition.y - Stage.currentRoom.objects.GetLength(1) / 2f, currentRoomPosition.y + Stage.currentRoom.objects.GetLength(1) / 2f);

        /*// Follow selected unit to the within a quarter of the screen in each direction
        if(followObject != null) {
            Vector2 destination = new Vector2(Mathf.Clamp(pos.x, followObject.transform.position.x - followLeniency * GetComponent<Camera>().orthographicSize * Screen.width / Screen.height, followObject.transform.position.x + followLeniency * GetComponent<Camera>().orthographicSize * Screen.width / Screen.height),
                                              Mathf.Clamp(pos.y, followObject.transform.position.y - followLeniency * GetComponent<Camera>().orthographicSize, followObject.transform.position.y + followLeniency * GetComponent<Camera>().orthographicSize));
            Vector2 v = Vector2.MoveTowards(pos, destination, followSpeed * Time.deltaTime);
            if(Vector2.Distance(v, new Vector2(pos.x, pos.y)) > 1.1f * Settings.s.unitSpeed * Time.deltaTime)
                moving = true;
            else
                moving = false;
            pos = new Vector3(v.x, v.y, -10);
        } else
            moving = false;*/
        if(transform.position == this.pos)
            unfollow();
    }

    void FixedUpdate() {
        // Camera Follow
        GetComponent<Camera>().orthographicSize = Mathf.Lerp(GetComponent<Camera>().orthographicSize, this.orthSize, Settings.Instance.zoomFollow);

        transform.position = Vector3.Lerp(transform.position, this.pos, Settings.Instance.panFollow);
    }

    public void follow(Object obj, float leniency) {
        this.followObject = obj;
        // this.orthSize = Mathf.Max(this.orthSize, 4f);
    }

    public void unfollow() {
        this.followObject = null;
    }

    public bool isFollowing() {
        return this.followObject != null;
    }

    void Awake() {
        _instance = this;
    }

    public static CameraController Instance { get {
        return _instance;
    } }
}