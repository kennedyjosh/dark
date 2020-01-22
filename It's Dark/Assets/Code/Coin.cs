using UnityEngine;

public class Coin : MonoBehaviour {

    public static int num_coins { private set; get; }
    private const int COIN_CAP = 4;
    
    
    // Update is called once per frame
    void Update() {
        if (!Game.current.playing) return;
        
        if (num_coins < 0.2f * (Game.TIME-Game.current.gametime) && num_coins < COIN_CAP) {
            NewCoin();
        }
    }

    public static void ResetStatics() {
        num_coins = 0;
    }

    public static Coin NewCoin() {
        float x = Random.Range(-8f, 8f);
        float y = Random.Range(-4.5f, 4f);
        var coinPrefab = Resources.Load("Coin");
        var coin = (GameObject)Instantiate(coinPrefab, new Vector3(x,y,1), Quaternion.identity);
        Coin c = coin.GetComponent<Coin>();
        num_coins += 1;
        return c;

    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!Game.current.playing) return;
        
        if (other.GetComponent<Player>()) {
            Game.current.GetCoin();
            Invoke("NewCoin", 0.5f);
            Invoke("NewCoin", 1f);
            SelfDestruct();
        }
    }

    private void SelfDestruct() {
        num_coins -= 1;
        GetComponent<Renderer>().enabled = false;
        Destroy(gameObject);
    }
}
