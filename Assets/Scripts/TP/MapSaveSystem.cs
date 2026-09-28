using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RobotPaint
{
    /// <summary>
    /// Sauvegarde et chargement des cartes.
    /// Les boutons de l'UI appellent déjà ces fonctions.
    /// </summary>
    public class MapSaveSystem : MonoBehaviour
    {
        [SerializeField] private MapPainter painter;
        [SerializeField] private UIManager ui;

        // Dossier où ranger les sauvegardes
        private string SaveFolder => Path.Combine(Application.persistentDataPath, "Saves");

        private void Start()
        {
            RefreshSaveList();
        }

        /// <summary>
        /// Sauvegarde la carte actuelle. Appelée par le bouton "Sauvegarder".
        /// </summary>
        public void SaveMap(string saveName)
        {
            // TODO : painter.GetMapTexture() -> EncodeToPNG() -> File.WriteAllBytes(...)
        }

        /// <summary>
        /// Retourne le nom de toutes les sauvegardes présentes dans SaveFolder.
        /// </summary>
        public List<string> GetAvailableSaves()
        {
            // TODO : Directory.GetFiles(...) -> Path.GetFileNameWithoutExtension(...)
            return new List<string>();
        }

        /// <summary>
        /// Affiche la liste des sauvegardes dans l'UI.
        /// Appelée au lancement du jeu, par le bouton "Rafraîchir" et après une sauvegarde.
        /// </summary>
        public void RefreshSaveList()
        {
            // TODO : ui.ClearSaveEntries() puis ui.CreateSaveEntry(nom) pour chaque sauvegarde
        }

        /// <summary>
        /// Charge une sauvegarde. Appelée quand on clique sur une sauvegarde de la liste.
        /// </summary>
        public void OnLoad(string saveName)
        {
            // TODO : File.ReadAllBytes(...) -> texture.LoadImage(...) -> painter.LoadFromTexture(texture)
        }
    }
}
