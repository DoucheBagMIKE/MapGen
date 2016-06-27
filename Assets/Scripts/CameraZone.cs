using UnityEngine;
using System.Collections;

[RequireComponent (typeof(BoxCollider2D))]
public class CameraZone : MonoBehaviour {

    public float zoneWidth;
    public float zoneHeight;

    BoxCollider2D zoneCollider;
    CameraController cam;
    public bool snapToMiddle;

	// Use this for initialization
	void Awake () {
        cam = FindObjectOfType<CameraController>();

        zoneCollider = GetComponent<BoxCollider2D>();

        zoneWidth = zoneCollider.size.x * transform.localScale.x;
        zoneHeight = zoneCollider.size.y * transform.localScale.y;

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Player")
        {
            cam.SetCamZone(this);
        }
    }
}
