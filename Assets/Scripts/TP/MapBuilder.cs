using Unity.AI.Navigation;
using UnityEngine;

namespace RobotPaint
{
    /// <summary>
    /// Construit la carte 3D à partir du dessin : un pixel = une case.
    /// </summary>
    public class MapBuilder : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject wallPrefab;
        public GameObject startPrefab;
        public GameObject goalPrefab;

        // TODO : ajouter les prefabs ScoreZone, DeathZone et SpeedBoostZone

        [Header("NavMesh")]
        public NavMeshSurface navMeshSurface;

        [Header("Résultat")]
        public bool hasStart;
        public bool hasGoal;
        public Vector3 startPosition;
        public Vector3 goalPosition;

        public void Build(Texture2D map, float cellSize)
        {
            hasStart = false;
            hasGoal = false;

            // On récupère tous les pixels du dessin en une seule fois.
            // Le pixel (x, y) est à l'index x + y * map.width.
            Color[] pixels = map.GetPixels();

            // On parcourt tous les pixels du dessin
            for (int x = 0; x < map.width; x++)
            {
                for (int y = 0; y < map.height; y++)
                {
                    Color color = pixels[x + y * map.width];
                    Vector3 position = MapGrid.CellToWorld(x, y, map.width, cellSize);

                    if (color == Color.black)
                    {
                        SpawnCell(wallPrefab, position, cellSize);
                    }
                    else if (color == Color.blue)
                    {
                        SpawnCell(startPrefab, position, cellSize);
                        startPosition = position;
                        hasStart = true;
                    }
                    else if (color == Color.magenta)
                    {
                        SpawnCell(goalPrefab, position, cellSize);
                        goalPosition = position;
                        hasGoal = true;
                    }

                    // TODO : Jaune = score, Rouge = mort, Vert = boost de vitesse
                    // Attention : Color.yellow n'est pas un jaune pur, utiliser new Color(1, 1, 0)
                }
            }

            // Une fois la carte construite, on génère le NavMesh pour que le robot puisse se déplacer
            navMeshSurface.BuildNavMesh();
        }

        public void Clear()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            navMeshSurface.RemoveData();
        }

        private void SpawnCell(GameObject prefab, Vector3 position, float cellSize)
        {
            GameObject cell = Instantiate(prefab, position, Quaternion.identity, transform);
            cell.transform.localScale = new Vector3(cellSize, 1, cellSize);
        }
    }
}
