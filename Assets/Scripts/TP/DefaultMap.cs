using UnityEngine;

namespace RobotPaint
{
    /// <summary>
    /// Carte de base : ce qui est dessiné au lancement du jeu.
    /// </summary>
    public static class DefaultMap
    {
        /// <summary>
        /// Dessine la carte de base dans la texture (32x32 pixels).
        /// </summary>
        public static void Draw(Texture2D map)
        {
            // Un tableau avec une couleur par pixel.
            // Le pixel (x, y) est à l'index x + y * map.width, et (0, 0) est en bas à gauche.
            Color[] pixels = new Color[map.width * map.height];

            // Pour l'instant, on remplit toute la carte en blanc (sol libre)
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            // TODO : dessiner une vraie carte de départ en modifiant le tableau
            // Exemples : un mur noir tout autour, un départ bleu, une arrivée magenta...

            // On envoie tout le tableau à la texture en une seule fois
            map.SetPixels(pixels);
        }
    }
}
