using UnityEngine;
using UnityEngine.UI;

namespace RobotPaint
{
    /// <summary>
    /// [CORE] Interface du jeu : panneau de dessin (gauche), panneau des sauvegardes (droite),
    /// panneau de jeu (haut) et message d'information (bas).
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Scripts")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MapPainter painter;
        [SerializeField] private MapSaveSystem saveSystem;

        [Header("Panneaux")]
        [SerializeField] private GameObject paintPanel;
        [SerializeField] private GameObject savePanel;
        [SerializeField] private GameObject playPanel;

        [Header("Dessin")]
        [SerializeField] private Button clearButton;
        [SerializeField] private Button validateButton;

        [Header("Jeu")]
        [SerializeField] private Button backToPaintButton;

        [Header("Sauvegardes")]
        [SerializeField] private InputField saveNameInput;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Transform saveListContent;
        [SerializeField] private Button saveEntryPrefab;

        [Header("Message")]
        [SerializeField] private Text statusText;

        private void Awake()
        {
            clearButton.onClick.AddListener(painter.Clear);
            validateButton.onClick.AddListener(gameManager.ValidateMap);
            backToPaintButton.onClick.AddListener(gameManager.BackToPaint);
            saveButton.onClick.AddListener(OnSaveClicked);
            refreshButton.onClick.AddListener(saveSystem.RefreshSaveList);

            statusText.text = "";
        }

        public void ShowPaintPhase()
        {
            paintPanel.SetActive(true);
            savePanel.SetActive(true);
            playPanel.SetActive(false);
        }

        public void ShowPlayPhase()
        {
            paintPanel.SetActive(false);
            savePanel.SetActive(false);
            playPanel.SetActive(true);
        }

        /// <summary>Affiche un message en bas de l'écran.</summary>
        public void ShowStatus(string message)
        {
            statusText.text = message;
        }

        /// <summary>Vide la liste des sauvegardes.</summary>
        public void ClearSaveEntries()
        {
            foreach (Transform entry in saveListContent)
                Destroy(entry.gameObject);
        }

        /// <summary>Ajoute une sauvegarde dans la liste. Un clic dessus appelle saveSystem.OnLoad(saveName).</summary>
        public void CreateSaveEntry(string saveName)
        {
            Button entry = Instantiate(saveEntryPrefab, saveListContent);
            entry.GetComponentInChildren<Text>().text = saveName;
            entry.onClick.AddListener(() => saveSystem.OnLoad(saveName));
        }

        private void OnSaveClicked()
        {
            saveSystem.SaveMap(saveNameInput.text);
            saveSystem.RefreshSaveList();
        }
    }
}
