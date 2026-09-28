using UnityEngine;
using UnityEngine.UI;

namespace RobotPaint
{
    /// <summary>
    /// [CORE] Bouton de la palette : choisit la couleur du pinceau.
    /// Pour ajouter une couleur : dupliquer un bouton dans Canvas/PaintPanel/Palette,
    /// puis changer "Color" ici et le texte du bouton.
    /// </summary>
    [RequireComponent(typeof(Button), typeof(Outline))]
    public class ColorButton : MonoBehaviour
    {
        [SerializeField] private MapPainter painter;
        [SerializeField] private Color color = Color.black;

        private static ColorButton selected; // Bouton actuellement sélectionné

        private void Start()
        {
            GetComponent<Image>().color = color;
            GetComponent<Outline>().enabled = false;
            GetComponent<Button>().onClick.AddListener(Select);

            if (color == painter.BrushColor)
                Select();
        }

        private void Select()
        {
            if (selected != null)
                selected.GetComponent<Outline>().enabled = false;

            selected = this;
            GetComponent<Outline>().enabled = true;
            painter.SetBrushColor(color);
        }
    }
}
