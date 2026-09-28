using UnityEngine;
using UnityEngine.AI;

namespace RobotPaint
{
    /// <summary>
    /// [CORE] Robot de la phase 2 : va du départ à l'arrivée avec un NavMeshAgent.
    ///
    /// Fonctions utilisables par les zones :
    ///   - ApplySpeedBoost(multiplicateur, durée)
    ///   - Kill() : le robot réapparaît au départ
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class RobotController : MonoBehaviour
    {
        [SerializeField] private float respawnDelay = 1f;

        private NavMeshAgent agent;
        private UIManager ui;
        private Vector3 startPosition;
        private Vector3 goalPosition;
        private float normalSpeed;
        private bool isDead;
        private bool hasArrived;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            normalSpeed = agent.speed;
        }

        /// <summary>Place le robot au départ et l'envoie vers l'arrivée.</summary>
        public void Init(Vector3 start, Vector3 goal, UIManager uiManager)
        {
            startPosition = start;
            goalPosition = goal;
            ui = uiManager;

            agent.Warp(startPosition);
            agent.SetDestination(goalPosition);
        }

        private void Update()
        {
            if (hasArrived)
                return;

            if (Vector3.Distance(transform.position, goalPosition) < 1f)
            {
                hasArrived = true;
                ui.ShowStatus("Arrivée atteinte !");
            }
        }

        /// <summary>Multiplie la vitesse du robot pendant quelques secondes.</summary>
        public void ApplySpeedBoost(float multiplier, float duration)
        {
            agent.speed = normalSpeed * multiplier;

            CancelInvoke(nameof(EndSpeedBoost));
            Invoke(nameof(EndSpeedBoost), duration);
        }

        /// <summary>Tue le robot : il s'arrête puis réapparaît au départ.</summary>
        public void Kill()
        {
            if (isDead)
                return;

            isDead = true;
            agent.isStopped = true;
            EndSpeedBoost();
            ui.ShowStatus("Le robot est mort !");

            Invoke(nameof(Respawn), respawnDelay);
        }

        private void EndSpeedBoost()
        {
            CancelInvoke(nameof(EndSpeedBoost));
            agent.speed = normalSpeed;
        }

        private void Respawn()
        {
            isDead = false;
            agent.Warp(startPosition);
            agent.isStopped = false;
            agent.SetDestination(goalPosition);
        }
    }
}
