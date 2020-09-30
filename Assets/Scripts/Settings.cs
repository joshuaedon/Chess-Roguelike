using UnityEngine;

[CreateAssetMenu(fileName = "New Settings Profile", menuName = "Settings Profile")]
public class Settings : ScriptableObject {
	public static Settings _instance;

	// Units
    public float unitSpeed;
	// Camera
	public int cameraFollowType; // 0 == All, 1 == Friendly, 2 == None
	// // Zoom
	public float minZoom;
	public float maxZoom;
	public float zoomSpeed;
	public float zoomFollow;
	// // Pan
	public float panSpeed;
	public float panFollow;

	public static Settings Instance { get {
		if(_instance == null)
			_instance = FindObjectOfType<Settings>();
		if(_instance == null)
			_instance = (Settings)Resources.Load("Settings/Default");
		return _instance;
	} }
}
