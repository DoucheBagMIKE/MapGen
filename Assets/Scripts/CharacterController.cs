using UnityEngine;
using System.Collections;

public class CharacterController : MonoBehaviour {

    Rigidbody2D rb2d;
    // Use this for initialization
    void Awake() {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate() {

        if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
        {
            rb2d.AddForce(new Vector2(Input.GetAxis("Horizontal")*16, Input.GetAxis("Vertical")*16));
        }
	
	}
}
