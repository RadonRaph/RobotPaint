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

            for (int x = 0; x < map.width; x++)
            {
                for (int y = 0; y < map.height; y++)
                {
                    int i = y*map.width + x;
                    Color color = Color.white;

                    Vector2 coord = new Vector2(i*0.1f+Random.value, y*0.1f + Random.value);
                    float value = Mathf.PerlinNoise(coord.x, coord.y);
                    float value2 = Mathf.PerlinNoise(coord.x + Random.value, coord.y + Random.value);

                    if (x == 0 || x ==map.width-1 || y == 0 || y == map.height-1 || value > 0.75f) //Murs
                    {
                        color = Color.black;
                        pixels[i] = color;
                        continue;
                    }

                    if (value < 0.15f) // Piéce
                    {
                        color = new Color(1, 1, 0);
                    }

                    if (value2 > 0.77f) // Death Zone
                    {
                        color = Color.red;
                    }else if (value2 < 0.12f) //Speed boost
                    {
                        color = Color.green;
                    }

                    pixels[i] = color;
                }
            }

            float distance = 0;
            int startRndX = 0;
            int startRndY = 0;
            int endRndX = 0;
            int endRndY = 0;

            while (distance < 20) //WARNING 
            {
                startRndX = Random.Range(10, map.width - 1);
                startRndY = Random.Range(10, map.height - 1);

                endRndX = Random.Range(1, map.width - 10);
                endRndY = Random.Range(1, map.height - 10);
                distance = Vector2.Distance(new Vector2(startRndX, startRndY), new Vector2(endRndX, endRndY));
            }

            pixels[startRndX + startRndY * map.width] = Color.blue;
            pixels[endRndX + endRndY * map.width] = Color.magenta;



            // On envoie tout le tableau à la texture en une seule fois
            map.SetPixels(pixels);
        }
    }
}
