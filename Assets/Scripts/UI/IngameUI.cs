using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MiniPlanetDefense
{
    /// <summary>
    /// Provides access to the ingame UI.
    /// </summary>
    public class IngameUI : MonoBehaviour
    {
        [SerializeField] Text textScore;
        [SerializeField] Text textTime;
        [SerializeField] GameObject introScreen;
        [SerializeField] GameObject restartScreen;
        [SerializeField] float restartScreenDelay = 1f;
        
        int score;
        int time;

        void Awake()
        {
            introScreen.SetActive(true);
            restartScreen.SetActive(false);
            UpdateScoreDisplay();
        }

        public void UpdateScoreDisplay()
        {
            textScore.text = ScoreManager.GetScore().ToString();
        }

        
        public void SetTime(int value)
        {
            if (time == value)
                return;

            time = value;
            textTime.text = time.ToString("00");
        }

        
        
        public void ShowRestartScreen()
        {
            StartCoroutine(ShowRestartScreenCoroutine());
        }

        IEnumerator ShowRestartScreenCoroutine()
        {
            yield return new WaitForSeconds(restartScreenDelay);
            
            restartScreen.gameObject.SetActive(true);
        }


        public void GoToMenuScene()
        {
            ScoreManager.SaveScore();
            SceneManager.LoadScene("StartMenu");
        }
    }
}