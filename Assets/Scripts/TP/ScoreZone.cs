using UnityEngine;

namespace RobotPaint
{
    /// <summary>
    /// Case Jaune : donne des points au robot puis disparaît.
    /// </summary>
    public class ScoreZone : MonoBehaviour
    {
        [SerializeField] private int points = 10;

        private void OnTriggerEnter(Collider other)
        {
            // TODO : si c'est le robot qui entre, ajouter les points au ScoreManager puis détruire la zone
        }
    }
}
