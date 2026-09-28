using UnityEngine;

namespace RobotPaint
{
    /// <summary>
    /// Case Rouge : tue le robot, qui réapparaît au départ.
    /// </summary>
    public class DeathZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            RobotController robot = other.GetComponent<RobotController>();
            if (robot == null)
                return;

            robot.Kill();
        }
    }
}
