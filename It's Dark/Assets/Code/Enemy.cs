using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour {

    public static int num_enemies { private set; get; }
    private const float COOLDOWN = 1.7f;
    private const int ENEMY_CAP = 10;
    private static int y_int = 1;
    
    private Transform _t;
    private AudioSource[] _noises;
    
    private float speed;

    void Start() {
        _t = GetComponent<Transform>();
        _noises = GetComponents<AudioSource>();
        speed = Random.Range(0.01f, 0.05f);
    }

    private void Update() {
        if (!Game.current.playing) return;

        if (num_enemies < 0.15f * (Game.TIME-Game.current.gametime) + y_int && num_enemies < ENEMY_CAP) {
            NewEnemy();
        }
        
    }

    void FixedUpdate() {
        if (!Game.current.playing) return;

        Vector3 playerPos = Game.current.PlayerPosition();
        Vector3 myPos = _t.position;
        float tmpSpeed = speed;

        // x
        float diffx = Math.Abs(playerPos[0] - myPos[0]);
        if (diffx < speed && diffx != 0) {
            speed = diffx;
        }

        if (playerPos[0] < myPos[0]) {
            MoveLeft();
        } else if (playerPos[0] > myPos[0]) {
            MoveRight();
        }

        if (diffx < speed) {
            speed = tmpSpeed;
        }

        // y
        float diffy = Math.Abs(playerPos[1] - myPos[1]);
        if (diffy < speed && diffy != 0) {
            speed = diffy;
        }

        if (playerPos[1] < myPos[1]) {
            MoveDown();
        } else if (playerPos[1] > myPos[1]) {
            MoveUp();   
        }
        
        speed = tmpSpeed;
    }

    public static void ResetStatics() {
        num_enemies = 0;
        y_int = 1;
    }

    public static Enemy NewEnemy(Vector3 pos) {
        var enemyPrefab = Resources.Load("Enemy");
        var enemy = (GameObject)Instantiate(enemyPrefab, pos, Quaternion.identity);
        Enemy e = enemy.GetComponent<Enemy>();
        num_enemies += 1;
        return e;
    }
    
    public static Enemy NewEnemy() {
        float x = Random.Range(-8f, 8f);
        float y = Random.Range(-4.3f, 3.8f);
        return NewEnemy(new Vector3(x, y, 1));
    }


    public static void StartGame() {
        NewEnemy(new Vector3(-8, 0, 1));
        NewEnemy(new Vector3(8, 0, 1));
    }

    private void MoveLeft() { _t.position += Vector3.left * speed; }

    private void MoveRight() { _t.position += Vector3.right * speed; }

    private void MoveUp() { _t.position += Vector3.up * speed; }

    private void MoveDown() { _t.position += Vector3.down * speed; }

    public void OnCollisionEnter2D(Collision2D other) {
        if (!Game.current.playing) return;
        
        if (other.gameObject.GetComponent<Player>()) {
            Game.current.LoseCoin();
            _noises[0].Play();
            GetComponent<Renderer>().enabled = false;
            GetComponent<CircleCollider2D>().enabled = false;
            Invoke("SelfDestruct", 1f);
        }
    }
    
    private void SelfDestruct() {
        num_enemies -= 1;
        y_int -= 1;
        Destroy(gameObject);
    }

}
