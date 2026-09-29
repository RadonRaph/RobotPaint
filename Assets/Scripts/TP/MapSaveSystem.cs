using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

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

            Texture2D texture = painter.GetMapTexture();
            byte[] octets = texture.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/saves/" + saveName + ".png", octets);
            // TODO : painter.GetMapTexture() -> EncodeToPNG() -> File.WriteAllBytes(...)
        }
        

        /// <summary>
        /// Retourne le nom de toutes les sauvegardes présentes dans SaveFolder.
        /// </summary>
        public List<string> GetAvailableSaves()
        {
            // TODO : Directory.GetFiles(...) -> Path.GetFileNameWithoutExtension(...)
            string[] files = Directory.GetFiles(Application.dataPath + "/saves");
            List<string> result = new List<string>();
            
            foreach(string file in files)
            {
                if (file.EndsWith(".png"))
                {
                    result.Add(Path.GetFileNameWithoutExtension(file));
                }
            }

            return result;
        }

        /// <summary>
        /// Affiche la liste des sauvegardes dans l'UI.
        /// Appelée au lancement du jeu, par le bouton "Rafraîchir" et après une sauvegarde.
        /// </summary>
        public void RefreshSaveList()
        {
            // TODO : ui.ClearSaveEntries() puis ui.CreateSaveEntry(nom) pour chaque sauvegarde
            ui.ClearSaveEntries();
            List<string> files = GetAvailableSaves();
            foreach (string file in files)
            {
                ui.CreateSaveEntry(file);
            }
        }

        /// <summary>
        /// Charge une sauvegarde. Appelée quand on clique sur une sauvegarde de la liste.
        /// </summary>
        public void OnLoad(string saveName)
        {
            // TODO : File.ReadAllBytes(...) -> texture.LoadImage(...) -> painter.LoadFromTexture(texture)
            byte[] octets = File.ReadAllBytes(Application.dataPath + "/saves/" + saveName + ".png");
            Texture2D texture = painter.GetMapTexture();
            texture.LoadImage(octets);
            painter.LoadFromTexture(texture);
        }
    }
}
