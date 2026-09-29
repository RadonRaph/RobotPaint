using UnityEngine;
using UnityEngine.UI;

namespace RobotPaint
{
    /// <summary>
    /// Garde le score de la partie et l'affiche à l'écran.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        // TODO : transformer ScoreManager en singleton pour pouvoir l'appeler depuis ScoreZone
        public static ScoreManager instance;

        [SerializeField] private Text scoreText;

        private int score;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this.gameObject);
            }
            else
            {
                instance = this;
            }
        }

        public void AddScore(int points)
        {
            score += points;
            scoreText.text = "Score : " + score;
        }

        public void ResetScore()
        {
            score = 0;
            scoreText.text = "Score : " + score;
        }
    }
}
