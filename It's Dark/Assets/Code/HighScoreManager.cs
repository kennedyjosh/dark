using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using UnityEngine;

public class HighScoreManager : MonoBehaviour {

    public static HighScoreManager current;
    
    public class Pair {
        public int score;
        public String plyr;

        public Pair(int s, String p) {
            score = s;
            plyr = p;
        }
    }
    
    private Pair[] scores;
    
    // Start is called before the first frame update
    void Start() {
        current = this;
        
        scores = new Pair[3];
        
        RefreshLeaderboard();
    }
    
    public int[] GetTopScores() {
        int[] ret = new int[3];
        for (int i = 0; i < 3; i++) {
            ret[i] = (scores[i].score);
        }
        return ret;
    }
    
    public String[] GetTopPlayers() {
        String[] ret = new String[3];
        for (int i = 0; i < 3; i++) {
            ret[i] = (scores[i].plyr);
        }

        return ret;
    }

    public void RefreshLeaderboard() {
        string html = string.Empty;
        string url = @"https://kennedyjosh.pythonanywhere.com/";
        //string url = "http://127.0.0.1:5000/";    // for debugging
        
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip;

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            html = reader.ReadToEnd();
        }

        String[] pairs = html.Split(';');
        String p;
        String[] pair;
        int n;
        for (int i = 0; i < 3; i++) {
            pair = pairs[i].Split(',');
            n = Int32.Parse(pair[0]);
            p = pair[1];
            scores[i] = new Pair(n, p);
        }
    }
    

    public bool NewHighScore(int s) {
        RefreshLeaderboard();
        return (s > scores[2].score);
    }
    
    public void SubmitNewScore(int s, String p) {
        PublishToWeb($"{s},{p}");
    }

    private void Sort() {
        Array.Sort(scores, delegate(Pair m, Pair n) { return n.score - m.score; });
    }

    public void Save() {
        Sort();

        String[] write = new string[3];
        for (int i = 0; i < 3; i++) {
            write[i] = $"{scores[i].score}" + "," + scores[i].plyr;
        }

        String pub = "";
        foreach (String s in write) {
            pub += s + ";";
        }
    }
    
    private async void PublishToWeb(String score) {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri("https://kennedyjosh.pythonanywhere.com/");
            //client.BaseAddress = new Uri("http://127.0.0.1:5000/");     // for debugging
            
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("data", score)
            });
            
            //Debug.Log(score);
            
            var result = await client.PostAsync("/", content);
            //client.PostAsync("/", content);
            
            RefreshLeaderboard();
            
            //string resultContent = await result.Content.ReadAsStringAsync();
            //Debug.Log(resultContent);
        }
    }

    
}
