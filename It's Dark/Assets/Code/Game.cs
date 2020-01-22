using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour {
    
    private const double VERSION = 1.0;

    public static Game current;
    public const int TIME = 45;
    private const int START_SCORE = 0;
    
    private Player _plyr;
    private Text _score;
    private Text _timer;
    private Text _display;
    private Text _hp;
    private Text _hs;
    private Text _instruct;
    private Text _restart;
    private InputField _inp;
    private AudioSource[] _noises;

    private int score;
    private bool waitingForInput;
    private bool endgame;
    private bool creepyNoiseNotPlayed;
    public bool playing { private set; get; }
    public float gametime { private set; get; }

    void Start() {
        current = this;
        
        // fix screen resolution
        Screen.SetResolution(884*2,494*2, false);
        
        // init variables
        _score = SetVar<Text>("Score");
        _timer = SetVar<Text>("Timer");
        _display = SetVar<Text>("Display");
        _noises = GetComponents<AudioSource>();
        _instruct = SetVar<Text>("Instruct");
        _inp = SetVar<InputField>("InputField");
        _hp = SetVar<Text>("Hnames");
        _hs = SetVar<Text>("Hscores");
        _restart = SetVar<Text>("Restart");
        
        // init player
        var playerPrefab = Resources.Load("Player");
        var player = (GameObject)Instantiate(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        _plyr = player.GetComponent<Player>();
        
        // init variables 
        playing = false;
        endgame = false;        
        waitingForInput = false;
        creepyNoiseNotPlayed = true;
        score = START_SCORE;
        gametime = TIME;
        ShowDisplayLabel();
    }
    

    void Update() {
        // handle case where the game is over and user is prompted to restart game
        if (endgame && !waitingForInput) {
            _restart.enabled = true;
            if (Input.GetAxisRaw("Jump") != 0) {
                Enemy.ResetStatics();
                Coin.ResetStatics();
                int scene = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(scene, LoadSceneMode.Single);
            }
        }
        
        // handle case when user is entering their initials for a high school
        String n;
        if (waitingForInput) {
            n = PromptName();
            if (n != "") {
                HighScoreManager.current.SubmitNewScore(score, n);
                _instruct.text = $"Your score: {score} coins";
                UpdateDisplayLabel("High Scores");
                StartCoroutine(ShowHighScores());
                ShowDisplayLabel();
            }
        }
        
        // handle case when game has not yet started and user is being prompted
        if (!playing && !endgame) {
            UpdateDisplayLabel("Press spacebar to begin");
            if (Input.GetAxisRaw("Jump") != 0) {
                playing = true;
                StartGame();
                HideDisplayLabel();
            }
            return;
        }
        
        // end function if game is not in a playing state
        if (!playing) return;
        
        // update time
        gametime -= Time.deltaTime;
        UpdateTimeLabel();
        if (gametime <= 0) {
            EndGame();
        }

        // play creepy noise when 1/3 of the time remains
        if (gametime <= (TIME/3) && creepyNoiseNotPlayed) {
            _noises[1].Play();
            creepyNoiseNotPlayed = false;
        }
        
        // if coins are all gone spawn a new one
        if (Coin.num_coins == 0) {
            Coin.NewCoin();
        }
        
        // if enemies are all gone spawn a new one
        if (Enemy.num_enemies == 0) {
            Enemy.NewEnemy();
        }
    }

    private T SetVar<T>(String name) {
        return GameObject.Find(name).GetComponent<T>();
    }

    private void StartGame() {
        _score.enabled = true;
        _timer.enabled = true;
        
        UpdateScoreLabel();
        UpdateTimeLabel();
        Enemy.StartGame();
        Coin.NewCoin();
    }

    
    private void EndGame() {
        endgame = true;
        playing = false; 
        if (score < 0) {
            UpdateDisplayLabel($"You lose");
            ShowDisplayLabel();
            _noises[2].Play();
            return;
        }
        if (HighScoreManager.current.NewHighScore(score)) {
            String n = PromptName();
            if (n == "") return;
            HighScoreManager.current.SubmitNewScore(score, n);
        }

        _instruct.text = $"Your score: {score} coins";
        _instruct.enabled = true;
        UpdateDisplayLabel($"High Scores");
        StartCoroutine(ShowHighScores());
        ShowDisplayLabel();
    }

    private String PromptName() {
        _instruct.enabled = true;
        _inp.enabled = true;
        waitingForInput = true;
        ShowDisplayLabel();

        _inp.Select();
        
        if (Input.GetAxisRaw("Submit") != 0 && _inp.text.Length > 0) {
            waitingForInput = false;
            _inp.enabled = false;
            return _inp.text;
        }

        return "";
    }

    public void GetCoin() {
        score += 1;
        _noises[0].Play();
        UpdateScoreLabel();
    }

    public void LoseCoin() {
        score -= 1;
        if (score < 0) {
            EndGame();
            return;
        }
        UpdateScoreLabel();
    }

    private void UpdateScoreLabel() {
        _score.text = $"Coins: {score}";
    }

    private void UpdateTimeLabel() {
        _timer.text = String.Format("Time: {0}", (int)gametime);
    }

    private void ShowDisplayLabel() {
        _display.enabled = true;
    }
    
    private void HideDisplayLabel() {
        _display.enabled = false;
    }

    private void UpdateDisplayLabel(String text) {
        _display.text = text;
    }

    private IEnumerator ShowHighScores() {
        yield return new WaitForSeconds(0.15f);
        HighScoreManager.current.RefreshLeaderboard();
        int[] s = HighScoreManager.current.GetTopScores();
        String[] p = HighScoreManager.current.GetTopPlayers();
        _hp.text = p[0]+'\n'+p[1]+'\n'+p[2];
        _hs.text = $"{s[0]}\n{s[1]}\n{s[2]}";
        _hp.enabled = true;
        _hs.enabled = true;
    }
    
    public Vector3 PlayerPosition() {
        return _plyr.CurrentPosition();
    }
}
