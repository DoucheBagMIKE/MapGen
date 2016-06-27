using UnityEngine;
using System.Collections;

public class MovementController : MonoBehaviour {

    public float moveSpd = 5f;

    Rigidbody2D rb;

	// Use this for initialization
	void Awake () {
        rb = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void Move(Vector2 moveVector)
    {
        rb.velocity += moveVector * moveSpd * Time.deltaTime;

        if (rb.velocity.magnitude > moveSpd)
            rb.velocity = rb.velocity.normalized * moveSpd;
    }
}
