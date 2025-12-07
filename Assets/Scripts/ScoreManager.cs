using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ScoreManager : MonoBehaviour
{
    [System.Serializable]
    public class ScoreData
    {
        public int score;
    }

    public int score = 0;
    public Text scoreText; // InspectorでTextをアサイン

    private string savePath;

    void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, "score.json");
        LoadScore();
        UpdateScoreText();
    }

    public void AddScore(int value)
    {
        score += value;
        UpdateScoreText();
        SaveScore();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    void SaveScore()
    {
        ScoreData data = new ScoreData();
        data.score = score;
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);
    }

    void LoadScore()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            ScoreData data = JsonUtility.FromJson<ScoreData>(json);
            score = data.score;
        }
    }
}
