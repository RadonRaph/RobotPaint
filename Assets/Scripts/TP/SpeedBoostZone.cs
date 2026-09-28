using UnityEngine;

namespace RobotPaint
{
    /// <summary>
    /// Case Verte : accélère le robot pendant quelques secondes.
    /// </summary>
    public class SpeedBoostZone : MonoBehaviour
    {
        [SerializeField] private float speedMultiplier = 2f;
        [SerializeField] private float duration = 3f;

        private void OnTriggerEnter(Collider other)
        {
            RobotController robot = other.GetComponent<RobotController>();
            if (robot == null)
                return;

            robot.ApplySpeedBoost(speedMultiplier, duration);
        }
    }
}
