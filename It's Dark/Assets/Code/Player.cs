using UnityEngine;

public class Player : MonoBehaviour {
    
    private const float SPEED = 0.075f;

    private Transform _t;

    private bool onGround;
    private float horiz;
    private float vert;

    void Start() {
        _t = GetComponent<Transform>();
    }

    void FixedUpdate() {
        horiz = Input.GetAxisRaw("Horizontal");
        vert = Input.GetAxisRaw("Vertical");

        if (horiz < 0) {
            MoveLeft();
        } else if (horiz > 0) {
            MoveRight();
        }

        if (vert < 0) {
            MoveDown();
        } else if (vert > 0) {
            MoveUp();
        }
    }

    public Vector3 CurrentPosition() { return _t.position; }
    
    private void MoveLeft() { _t.position += Vector3.left * SPEED; }

    private void MoveRight() { _t.position += Vector3.right * SPEED; }

    private void MoveUp() { _t.position += Vector3.up * SPEED; }

    private void MoveDown() { _t.position += Vector3.down * SPEED; }

}
