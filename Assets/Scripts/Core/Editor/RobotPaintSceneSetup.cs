using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace RobotPaint.EditorTools
{
    /// <summary>
    /// [CORE - Éditeur] Menu "RobotPaint > Construire la scène".
    ///
    /// Génère les matériaux (Assets/Materials), les prefabs (Assets/Prefabs) et la scène
    /// Assets/Scenes/RobotPaint.unity avec l'UI complète et toutes les références assignées.
    /// Attention : relancer le menu ÉCRASE la scène et les prefabs.
    /// </summary>
    public static class RobotPaintSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/RobotPaint.unity";
        private const string MaterialsFolder = "Assets/Materials";
        private const string PrefabsFolder = "Assets/Prefabs";

        // Doivent correspondre aux valeurs par défaut du MapPainter
        private const int Resolution = 32;
        private const float CellSize = 2f;

        private static readonly Color PanelColor = new Color(0.1f, 0.1f, 0.13f, 0.85f);
        private static readonly Color ButtonColor = new Color(0.85f, 0.85f, 0.9f);
        private static readonly Color ValidateColor = new Color(0.35f, 0.8f, 0.4f);
        private static readonly Color SelectionColor = new Color(1f, 0.6f, 0f);

        private class Materials
        {
            public Material Paint, Wall, Robot, RobotEye, Score, Death, Boost, Start, Goal;
        }

        private class Prefabs
        {
            public GameObject Wall, Robot, ScoreZone, DeathZone, BoostZone, StartMarker, GoalMarker, SaveEntry;
        }

        private static Font uiFont;
        private static DefaultControls.Resources uiResources;

        [MenuItem("RobotPaint/Construire la scène")]
        public static void BuildScene()
        {
            if (!EditorUtility.DisplayDialog("RobotPaint",
                    "Générer la scène, les prefabs et les matériaux ?\n\n" +
                    "Les fichiers existants (RobotPaint.unity, Assets/Prefabs, Assets/Materials) seront écrasés.",
                    "Générer", "Annuler"))
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EnsureFolder("Materials");
            EnsureFolder("Prefabs");
            EnsureFolder("Scenes");

            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiResources = CreateUIResources();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Materials mats = CreateMaterials();
            Prefabs prefabs = CreatePrefabs(mats);

            // --- Environnement ---
            Camera cam = CreateCamera();
            CreateLight();

            // --- Surface de peinture ---
            var paintQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            paintQuad.name = "PaintCanvas";
            Object.DestroyImmediate(paintQuad.GetComponent<Collider>());
            paintQuad.transform.SetPositionAndRotation(new Vector3(0f, 0.01f, 0f), Quaternion.Euler(90f, 0f, 0f));
            paintQuad.transform.localScale = new Vector3(Resolution * CellSize, Resolution * CellSize, 1f);
            paintQuad.GetComponent<Renderer>().sharedMaterial = mats.Paint;
            var painter = paintQuad.AddComponent<MapPainter>();
            SetRef(painter, "targetCamera", cam);

            // --- Sol : grand collider invisible utilisé par le NavMesh (le visuel est le Quad de peinture) ---
            var floor = new GameObject("Floor").AddComponent<BoxCollider>();
            floor.center = new Vector3(0f, -0.05f, 0f);
            floor.size = new Vector3(Resolution * CellSize, 0.1f, Resolution * CellSize);

            // --- Construction de la carte + NavMesh (collecte de tous les colliders de la scène) ---
            var builderGo = new GameObject("MapBuilder");
            var surface = builderGo.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

            var builder = builderGo.AddComponent<MapBuilder>();
            builder.wallPrefab = prefabs.Wall;
            builder.startPrefab = prefabs.StartMarker;
            builder.goalPrefab = prefabs.GoalMarker;
            builder.navMeshSurface = surface;

            // --- Managers ---
            var managersGo = new GameObject("GameManager");
            var gameManager = managersGo.AddComponent<GameManager>();
            var scoreManager = managersGo.AddComponent<ScoreManager>();
            var saveSystem = managersGo.AddComponent<MapSaveSystem>();

            // --- UI ---
            UIManager ui = CreateUI(gameManager, painter, saveSystem, scoreManager, prefabs.SaveEntry);

            SetRef(gameManager, "painter", painter);
            SetRef(gameManager, "mapBuilder", builder);
            SetRef(gameManager, "robotPrefab", prefabs.Robot.GetComponent<RobotController>());
            SetRef(gameManager, "ui", ui);
            SetRef(gameManager, "scoreManager", scoreManager);
            SetRef(saveSystem, "painter", painter);
            SetRef(saveSystem, "ui", ui);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[RobotPaint] Scène générée : {ScenePath}");
        }

        // =====================================================================
        //  Matériaux
        // =====================================================================

        private static Materials CreateMaterials()
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");

            return new Materials
            {
                Paint = GetOrCreateMaterial("M_PaintCanvas", unlit, Color.white),
                Wall = GetOrCreateMaterial("M_Wall", lit, new Color(0.2f, 0.2f, 0.25f)),
                Robot = GetOrCreateMaterial("M_Robot", lit, new Color(1f, 0.55f, 0.1f)),
                RobotEye = GetOrCreateMaterial("M_RobotEye", lit, new Color(0.1f, 0.1f, 0.1f)),
                Score = GetOrCreateMaterial("M_ScoreZone", lit, new Color(1f, 1f, 0f)),
                Death = GetOrCreateMaterial("M_DeathZone", lit, Color.red),
                Boost = GetOrCreateMaterial("M_SpeedBoostZone", lit, Color.green),
                Start = GetOrCreateMaterial("M_StartMarker", lit, Color.blue),
                Goal = GetOrCreateMaterial("M_GoalMarker", lit, Color.magenta),
            };
        }

        private static Material GetOrCreateMaterial(string name, Shader shader, Color color)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.shader = shader;
            mat.color = color;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        // =====================================================================
        //  Prefabs
        // =====================================================================

        private static Prefabs CreatePrefabs(Materials mats)
        {
            var p = new Prefabs();

            // Mur : cube 1 x 2 x 1, zone NavMesh "Not Walkable"
            var wall = new GameObject("Wall");
            AddVisual(wall, PrimitiveType.Cube, new Vector3(0f, 1f, 0f), new Vector3(1f, 2f, 1f), mats.Wall, keepCollider: true);
            var wallModifier = wall.AddComponent<NavMeshModifier>();
            wallModifier.overrideArea = true;
            wallModifier.area = 1; // 1 = "Not Walkable"
            p.Wall = SavePrefab(wall, "Wall");

            // Marqueurs de départ / arrivée (visuels uniquement)
            p.StartMarker = SavePrefab(CreateFlatTile("StartMarker", mats.Start), "StartMarker");
            p.GoalMarker = SavePrefab(CreateFlatTile("GoalMarker", mats.Goal), "GoalMarker");

            // Zones spéciales
            var score = CreateZoneRoot("ScoreZone");
            AddVisual(score, PrimitiveType.Sphere, new Vector3(0f, 0.8f, 0f), Vector3.one * 0.6f, mats.Score);
            score.AddComponent<ScoreZone>();
            p.ScoreZone = SavePrefab(score, "ScoreZone");

            var death = CreateZoneRoot("DeathZone");
            AddVisual(death, PrimitiveType.Cube, new Vector3(0f, 0.03f, 0f), new Vector3(0.95f, 0.06f, 0.95f), mats.Death);
            death.AddComponent<DeathZone>();
            p.DeathZone = SavePrefab(death, "DeathZone");

            var boost = CreateZoneRoot("SpeedBoostZone");
            AddVisual(boost, PrimitiveType.Cube, new Vector3(0f, 0.03f, 0f), new Vector3(0.95f, 0.06f, 0.95f), mats.Boost);
            boost.AddComponent<SpeedBoostZone>();
            p.BoostZone = SavePrefab(boost, "SpeedBoostZone");

            // Robot : un cylindre qui fait exactement une case de large
            float robotRadius = CellSize * 0.5f;
            var robot = new GameObject("Robot");
            AddVisual(robot, PrimitiveType.Cylinder, new Vector3(0f, 0.6f, 0f), new Vector3(CellSize, 0.6f, CellSize), mats.Robot);
            AddVisual(robot, PrimitiveType.Cube, new Vector3(0f, 1.2f, robotRadius * 0.6f), new Vector3(CellSize * 0.5f, 0.2f, 0.3f), mats.RobotEye);

            var robotCollider = robot.AddComponent<CapsuleCollider>();
            robotCollider.center = new Vector3(0f, robotRadius, 0f);
            robotCollider.radius = robotRadius * 0.9f;
            robotCollider.height = CellSize;

            // Rigidbody cinématique : nécessaire pour déclencher les OnTriggerEnter des zones
            var rb = robot.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var agent = robot.AddComponent<NavMeshAgent>();
            agent.radius = robotRadius;
            agent.height = 1.2f;
            agent.speed = 6f;
            agent.angularSpeed = 540f;
            agent.acceleration = 30f;
            agent.stoppingDistance = 0f;

            robot.AddComponent<RobotController>();
            p.Robot = SavePrefab(robot, "Robot");

            // Entrée de la liste des sauvegardes (UI)
            p.SaveEntry = SavePrefab(CreateButton(null, "SaveEntry", "Nom de la sauvegarde", 40f, ButtonColor), "SaveEntry");

            return p;
        }

        private static GameObject CreateFlatTile(string name, Material mat)
        {
            var root = new GameObject(name);
            AddVisual(root, PrimitiveType.Cube, new Vector3(0f, 0.03f, 0f), new Vector3(0.95f, 0.06f, 0.95f), mat);
            return root;
        }

        /// <summary>Racine d'une zone : trigger que le robot traverse, ignoré par le NavMesh.</summary>
        private static GameObject CreateZoneRoot(string name)
        {
            var root = new GameObject(name);
            var box = root.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0f, 1f, 0f);
            box.size = new Vector3(0.8f, 2f, 0.8f);

            root.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            return root;
        }

        /// <summary>Ajoute une primitive enfant servant de visuel (sans collider par défaut).</summary>
        private static GameObject AddVisual(GameObject parent, PrimitiveType type, Vector3 localPos, Vector3 localScale,
            Material mat, bool keepCollider = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = "Visual";
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!keepCollider)
                Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static GameObject SavePrefab(GameObject root, string name)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabsFolder}/{name}.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        // =====================================================================
        //  Caméra / lumière
        // =====================================================================

        private static Camera CreateCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();

            // Vue du dessus orthographique : le haut de l'écran correspond à +Z
            go.transform.SetPositionAndRotation(new Vector3(0f, 100f, 0f), Quaternion.Euler(90f, 0f, 0f));
            cam.orthographic = true;
            cam.orthographicSize = Resolution * CellSize * 0.55f; // Marge pour les panneaux latéraux
            cam.farClipPlane = 300f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.15f);
            return cam;
        }

        private static void CreateLight()
        {
            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // =====================================================================
        //  UI
        // =====================================================================

        private static UIManager CreateUI(GameManager gameManager, MapPainter painter, MapSaveSystem saveSystem,
            ScoreManager scoreManager, GameObject saveEntryPrefab)
        {
            // Canvas + EventSystem (Input System)
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            Transform canvas = canvasGo.transform;

            // ---------------- Panneau DESSIN (gauche) ----------------
            RectTransform paintPanel = CreatePanel(canvas, "PaintPanel",
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(400f, -40f));

            CreateText(paintPanel, "Title", "Phase 1 : Dessin", 34, FontStyle.Bold);
            CreateText(paintPanel, "Help", "Clic gauche : peindre\nClic droit : gommer", 20, FontStyle.Italic);
            CreateText(paintPanel, "PaletteLabel", "Couleurs", 24, FontStyle.Bold);

            var palette = new GameObject("Palette", typeof(RectTransform), typeof(GridLayoutGroup));
            palette.transform.SetParent(paintPanel, false);
            var grid = palette.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(112f, 64f);
            grid.spacing = new Vector2(10f, 10f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            CreateColorButton(palette.transform, painter, "Libre", Color.white, Color.black);
            CreateColorButton(palette.transform, painter, "Mur", Color.black, Color.white);
            CreateColorButton(palette.transform, painter, "Départ", Color.blue, Color.white);
            CreateColorButton(palette.transform, painter, "Arrivée", Color.magenta, Color.white);

            AddFlexibleSpace(paintPanel);
            Button clearButton = CreateButton(paintPanel, "ClearButton", "Tout effacer", 50f, ButtonColor).GetComponent<Button>();
            Button validateButton = CreateButton(paintPanel, "ValidateButton", "Valider la carte", 70f, ValidateColor).GetComponent<Button>();

            // ---------------- Panneau SAUVEGARDES (droite) ----------------
            RectTransform savePanel = CreatePanel(canvas, "SavePanel",
                new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(400f, -40f));

            CreateText(savePanel, "Title", "Sauvegardes", 34, FontStyle.Bold);

            GameObject inputGo = DefaultControls.CreateInputField(uiResources);
            inputGo.name = "SaveNameInput";
            inputGo.transform.SetParent(savePanel, false);
            SetHeight(inputGo, 50f);
            var input = inputGo.GetComponent<InputField>();
            var placeholder = (Text)input.placeholder;
            placeholder.text = "Nom de la carte...";
            placeholder.fontSize = 22;
            input.textComponent.fontSize = 22;

            Button saveButton = CreateButton(savePanel, "SaveButton", "Sauvegarder", 50f, ValidateColor).GetComponent<Button>();
            Button refreshButton = CreateButton(savePanel, "RefreshButton", "Rafraîchir la liste", 50f, ButtonColor).GetComponent<Button>();
            CreateText(savePanel, "ListLabel", "Cartes disponibles :", 22, FontStyle.Bold);

            GameObject scrollGo = DefaultControls.CreateScrollView(uiResources);
            scrollGo.name = "SaveList";
            scrollGo.transform.SetParent(savePanel, false);
            var scrollLayout = scrollGo.AddComponent<LayoutElement>();
            scrollLayout.minHeight = 200f;
            scrollLayout.flexibleHeight = 1f;
            var scrollRect = scrollGo.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            Object.DestroyImmediate(scrollRect.horizontalScrollbar.gameObject);
            scrollRect.horizontalScrollbar = null;

            // Le "Content" s'agrandit automatiquement avec les entrées de la liste
            RectTransform content = scrollRect.content;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(6, 6, 6, 6);
            contentLayout.spacing = 6f;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ---------------- Panneau JEU (haut) ----------------
            RectTransform playPanel = CreatePanel(canvas, "PlayPanel",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(560f, 210f));
            CreateText(playPanel, "Title", "Phase 2 : Le robot en route !", 30, FontStyle.Bold);
            Text scoreText = CreateText(playPanel, "ScoreText", "Score : 0", 40, FontStyle.Bold);
            scoreText.color = new Color(1f, 0.9f, 0.2f);
            Button backButton = CreateButton(playPanel, "BackToPaintButton", "Retour à l'édition", 55f, ButtonColor).GetComponent<Button>();

            // ---------------- Message (bas) ----------------
            var statusGo = new GameObject("StatusText", typeof(RectTransform));
            statusGo.transform.SetParent(canvas, false);
            var statusRect = (RectTransform)statusGo.transform;
            statusRect.anchorMin = statusRect.anchorMax = new Vector2(0.5f, 0f);
            statusRect.pivot = new Vector2(0.5f, 0f);
            statusRect.anchoredPosition = new Vector2(0f, 30f);
            statusRect.sizeDelta = new Vector2(1000f, 60f);
            var statusText = statusGo.AddComponent<Text>();
            statusText.fontSize = 30;
            statusText.fontStyle = FontStyle.Bold;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.color = Color.white;
            statusText.raycastTarget = false;
            var statusOutline = statusGo.AddComponent<Outline>();
            statusOutline.effectColor = Color.black;
            statusOutline.effectDistance = new Vector2(2f, -2f);

            // Même police partout
            foreach (Text t in canvasGo.GetComponentsInChildren<Text>(true))
                t.font = uiFont;

            // ---------------- Références ----------------
            SetRef(scoreManager, "scoreText", scoreText);

            var ui = canvasGo.AddComponent<UIManager>();
            SetRef(ui, "gameManager", gameManager);
            SetRef(ui, "painter", painter);
            SetRef(ui, "saveSystem", saveSystem);
            SetRef(ui, "paintPanel", paintPanel.gameObject);
            SetRef(ui, "savePanel", savePanel.gameObject);
            SetRef(ui, "playPanel", playPanel.gameObject);
            SetRef(ui, "clearButton", clearButton);
            SetRef(ui, "validateButton", validateButton);
            SetRef(ui, "backToPaintButton", backButton);
            SetRef(ui, "saveNameInput", input);
            SetRef(ui, "saveButton", saveButton);
            SetRef(ui, "refreshButton", refreshButton);
            SetRef(ui, "saveListContent", content);
            SetRef(ui, "saveEntryPrefab", saveEntryPrefab.GetComponent<Button>());
            SetRef(ui, "statusText", statusText);

            playPanel.gameObject.SetActive(false);
            return ui;
        }

        private static DefaultControls.Resources CreateUIResources()
        {
            return new DefaultControls.Resources
            {
                standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
                background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
                inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
                knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
                checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
                dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
                mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd"),
            };
        }

        /// <summary>Panneau avec fond sombre qui empile ses enfants verticalement.</summary>
        private static RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var image = go.GetComponent<Image>();
            image.sprite = uiResources.background;
            image.type = Image.Type.Sliced;
            image.color = PanelColor;

            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return rect;
        }

        private static Text CreateText(Transform parent, string name, string content, int size, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = uiFont;
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreateButton(Transform parent, string name, string label, float height, Color color)
        {
            GameObject go = DefaultControls.CreateButton(uiResources);
            go.name = name;
            if (parent != null)
                go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            SetHeight(go, height);

            Text text = go.GetComponentInChildren<Text>();
            text.font = uiFont;
            text.text = label;
            text.fontSize = 24;
            text.color = Color.black;
            return go;
        }

        private static void CreateColorButton(Transform parent, MapPainter painter, string label, Color color, Color textColor)
        {
            GameObject go = CreateButton(parent, "Color_" + label, label, 64f, color);
            go.GetComponentInChildren<Text>().color = textColor;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = SelectionColor;
            outline.effectDistance = new Vector2(5f, -5f);

            var colorButton = go.AddComponent<ColorButton>();
            SetRef(colorButton, "painter", painter);
            var so = new SerializedObject(colorButton);
            so.FindProperty("color").colorValue = color;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetHeight(GameObject go, float height)
        {
            var layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
            layout.minHeight = height;
        }

        private static void AddFlexibleSpace(Transform parent)
        {
            var go = new GameObject("Space", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().flexibleHeight = 1f;
        }

        // =====================================================================
        //  Utilitaires
        // =====================================================================

        /// <summary>Assigne un champ [SerializeField] privé.</summary>
        private static void SetRef(Object target, string propertyName, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propertyName).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string name)
        {
            if (!AssetDatabase.IsValidFolder("Assets/" + name))
                AssetDatabase.CreateFolder("Assets", name);
        }

        private static void AddSceneToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == path))
                return;

            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
