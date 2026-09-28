using UnityEngine;

namespace RobotPaint
{
    /// <summary>
    /// [CORE] Gère le passage entre les deux phases du jeu :
    ///   Phase 1 (dessin)  --Valider-->  Phase 2 (le robot se déplace)  --Retour-->  Phase 1
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private MapPainter painter;
        [SerializeField] private MapBuilder mapBuilder;
        [SerializeField] private RobotController robotPrefab;
        [SerializeField] private UIManager ui;
        [SerializeField] private ScoreManager scoreManager;

        private RobotController robot;

        private void Start()
        {
            ui.ShowPaintPhase();
        }

        /// <summary>Bouton "Valider" : construit la carte et lance le robot.</summary>
        public void ValidateMap()
        {
            mapBuilder.Build(painter.GetMapTexture(), painter.CellSize);

            if (!mapBuilder.hasStart || !mapBuilder.hasGoal)
            {
                mapBuilder.Clear();
                ui.ShowStatus("Il faut un départ (bleu) et une arrivée (magenta) !");
                return;
            }

            scoreManager.ResetScore();

            robot = Instantiate(robotPrefab, mapBuilder.startPosition, Quaternion.identity);
            robot.Init(mapBuilder.startPosition, mapBuilder.goalPosition, ui);

            painter.enabled = false;
            ui.ShowPlayPhase();
            ui.ShowStatus("C'est parti !");
        }

        /// <summary>Bouton "Retour à l'édition" : supprime le robot et la carte 3D (le dessin est conservé).</summary>
        public void BackToPaint()
        {
            if (robot != null)
                Destroy(robot.gameObject);

            mapBuilder.Clear();

            painter.enabled = true;
            ui.ShowPaintPhase();
            ui.ShowStatus("");
        }
    }
}
