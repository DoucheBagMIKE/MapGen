using UnityEngine;
using System.Collections;

[RequireComponent (typeof(MovementController))]
public class PlayerInputController : MonoBehaviour {

    Vector2 axisInput;
    bool fire1;
    bool fire2;

    MovementController mc;

	// Use this for initialization
	void Awake () {

        mc = GetComponent<MovementController>();
	}
	
	// Update is called once per frame
	void Update () {

        axisInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        fire1 = Input.GetButton("Fire1");
        fire2 = Input.GetButton("Fire2");

        mc.Move(axisInput);

    }
}
