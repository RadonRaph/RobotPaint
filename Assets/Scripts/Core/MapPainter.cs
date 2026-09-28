using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RobotPaint
{
    /// <summary>
    /// [CORE] Phase 1 : dessin de la carte à la souris (clic gauche = peindre, clic droit = gommer).
    ///
    /// Les pixels sont écrits dans une Texture2D de 32x32 (lisible par le code),
    /// puis copiés dans une RenderTexture affichée sur le Quad du sol.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class MapPainter : MonoBehaviour
    {
        [Header("Grille")]
        [SerializeField] private int resolution = 32;
        [Tooltip("Taille d'une case en unités monde (le robot fait une case de large).")]
        [SerializeField] private float cellSize = 2f;

        [Header("Pinceau")]
        [SerializeField] private Color brushColor = Color.black;

        [SerializeField] private Camera targetCamera;

        public int Resolution => resolution;
        public float CellSize => cellSize;
        public Color BrushColor => brushColor;

        private Texture2D mapTexture;
        private RenderTexture renderTexture;
        private Vector2Int? lastCell; // Dernière case peinte, pour tracer un trait continu

        private void Awake()
        {
            // Filtrage "Point" pour garder des pixels nets
            mapTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            mapTexture.filterMode = FilterMode.Point;

            renderTexture = new RenderTexture(resolution, resolution, 0);
            renderTexture.filterMode = FilterMode.Point;

            GetComponent<Renderer>().material.mainTexture = renderTexture;

            // Le Quad est posé à plat et fait exactement la taille de la grille
            float worldSize = resolution * cellSize;
            transform.SetPositionAndRotation(new Vector3(0f, 0.01f, 0f), Quaternion.Euler(90f, 0f, 0f));
            transform.localScale = new Vector3(worldSize, worldSize, 1f);

            Clear();
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            bool paint = mouse.leftButton.isPressed;
            bool erase = mouse.rightButton.isPressed;

            if ((!paint && !erase) || EventSystem.current.IsPointerOverGameObject())
            {
                lastCell = null;
                return;
            }

            if (!TryGetCellUnderMouse(mouse.position.ReadValue(), out Vector2Int cell))
            {
                lastCell = null;
                return;
            }

            Color color = erase ? Color.white : brushColor;

            // On relie la case précédente à la case actuelle pour éviter les trous quand la souris va vite
            Vector2Int from = lastCell ?? cell;
            int steps = Mathf.Max(Mathf.Abs(cell.x - from.x), Mathf.Abs(cell.y - from.y));
            for (int i = 0; i <= steps; i++)
            {
                Vector2 p = Vector2.Lerp(from, cell, steps == 0 ? 0f : (float)i / steps);
                mapTexture.SetPixel(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), color);
            }

            lastCell = cell;
            ApplyChanges();
        }

        // ---------------------------------------------------------------------
        //  Fonctions utilisables par les autres scripts
        // ---------------------------------------------------------------------

        public void SetBrushColor(Color color)
        {
            brushColor = color;
        }

        /// <summary>Remet toute la carte en blanc.</summary>
        public void Clear()
        {
            Color[] pixels = new Color[resolution * resolution];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;

            mapTexture.SetPixels(pixels);
            ApplyChanges();
        }

        /// <summary>Texture de la carte actuelle (32x32).</summary>
        public Texture2D GetMapTexture()
        {
            return mapTexture;
        }

        /// <summary>Remplace la carte par le contenu d'une texture de même taille.</summary>
        public void LoadFromTexture(Texture2D texture)
        {
            if (texture.width != resolution || texture.height != resolution)
            {
                Debug.LogError($"La texture doit faire {resolution}x{resolution} pixels.");
                return;
            }

            mapTexture.SetPixels(texture.GetPixels());
            ApplyChanges();
        }

        // ---------------------------------------------------------------------
        //  Interne
        // ---------------------------------------------------------------------

        /// <summary>Trouve la case sous la souris en lançant un rayon sur le plan du sol.</summary>
        private bool TryGetCellUnderMouse(Vector2 screenPosition, out Vector2Int cell)
        {
            cell = default;
            Ray ray = targetCamera.ScreenPointToRay(screenPosition);
            Plane ground = new Plane(Vector3.up, Vector3.zero);

            if (!ground.Raycast(ray, out float distance))
                return false;

            if (!MapGrid.WorldToCell(ray.GetPoint(distance), resolution, cellSize, out int x, out int y))
                return false;

            cell = new Vector2Int(x, y);
            return true;
        }

        /// <summary>Envoie la texture à la carte graphique et la copie dans la RenderTexture affichée.</summary>
        private void ApplyChanges()
        {
            mapTexture.Apply();
            Graphics.Blit(mapTexture, renderTexture);
        }
    }
}
