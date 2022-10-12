using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Image fadeImage;
    public GameObject gameOverUI;
    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<Player>().OnDeath.AddListener(GameOver);   
    }

    void GameOver()
    {
        StartCoroutine(Fade(Color.clear, Color.black, 2));
        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadeImage.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    public void RestartGame()
    {
        Debug.Log("R");
        SceneManager.LoadScene("MapGenerator");
    }
        
}
