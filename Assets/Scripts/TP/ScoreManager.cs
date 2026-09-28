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

        [SerializeField] private Text scoreText;

        private int score;

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
