using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform targetTransform;

    public float lerpSpd = 0.5f;

    public CameraZone curCamZone;
    public Vector2 min;
    public Vector2 max;

    public float verExtent;
    public float horExtent;
    Camera cam;

    // Use this for initialization
    void Awake () {

        cam = Camera.main;

        verExtent = cam.orthographicSize;
        horExtent = verExtent * Screen.width / Screen.height;

    }
	
	// Update is called once per frame
	void FixedUpdate () {


        Vector3 clampedPos = new Vector3(Mathf.Clamp(targetTransform.position.x, min.x + horExtent, max.x - horExtent), Mathf.Clamp(targetTransform.position.y, min.y + verExtent, max.y - verExtent), -10);

        Vector3 lerpedPos = Vector3.Lerp(transform.position, clampedPos, lerpSpd);
        lerpedPos.z = -10;
        transform.position = lerpedPos;
	
	}

    public void SetCamZone(CameraZone zone)
    {
        curCamZone = zone;

        /* // centered hotspot
        min.x = curCamZone.transform.position.x - ((curCamZone.zoneWidth / 2));
        max.x = curCamZone.transform.position.x + (curCamZone.zoneWidth / 2);
        min.y = curCamZone.transform.position.y - (curCamZone.zoneHeight / 2);
        max.y = curCamZone.transform.position.y + (curCamZone.zoneHeight / 2);
        */


        // top left hotspot
        min.x = curCamZone.transform.position.x;
        max.x = curCamZone.transform.position.x + (curCamZone.zoneWidth);
        min.y = curCamZone.transform.position.y - (curCamZone.zoneHeight);
        max.y = curCamZone.transform.position.y;
    }
}
