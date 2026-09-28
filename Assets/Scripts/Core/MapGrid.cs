using UnityEngine;

namespace RobotPaint
{
    /// <summary>
    /// [CORE] Conversions entre une case de la grille (x, y) et une position dans le monde.
    ///
    /// La carte est centrée sur l'origine et posée sur le plan Y = 0.
    /// L'axe X de la texture correspond à l'axe X du monde, l'axe Y de la texture à l'axe Z du monde.
    /// </summary>
    public static class MapGrid
    {
        /// <summary>Centre de la case (x, y) dans le monde.</summary>
        public static Vector3 CellToWorld(int x, int y, int resolution, float cellSize)
        {
            float half = resolution * 0.5f;
            return new Vector3((x + 0.5f - half) * cellSize, 0f, (y + 0.5f - half) * cellSize);
        }

        /// <summary>Case contenant la position monde. Retourne false si on est hors de la carte.</summary>
        public static bool WorldToCell(Vector3 worldPos, int resolution, float cellSize, out int x, out int y)
        {
            float half = resolution * 0.5f;
            x = Mathf.FloorToInt(worldPos.x / cellSize + half);
            y = Mathf.FloorToInt(worldPos.z / cellSize + half);
            return x >= 0 && x < resolution && y >= 0 && y < resolution;
        }
    }
}
