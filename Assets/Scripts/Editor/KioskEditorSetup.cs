using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Kiosk.Core;

namespace Kiosk.EditorTools
{
    /// <summary>
    /// Editor-Tool (Menue "Kiosk"): erzeugt optional alle Platzhalter-Assets als
    /// Dateien (ScriptableObjects, Materialien, Icon-Texturen) und kann die
    /// beiden Szenen neu generieren, falls sie beschaedigt wurden.
    ///
    /// Hinweis: Das Spiel ist auch OHNE diese Schritte sofort spielbar, weil
    /// SceneBootstrapper alles zur Laufzeit erzeugt. Die exportierten Assets
    /// dienen als Basis, um spaeter hochwertige Assets einzusetzen.
    /// </summary>
    public static class KioskEditorSetup
    {
        [MenuItem("Kiosk/Alle Platzhalter-Assets generieren")]
        public static void GenerateAllAssets()
        {
            GenerateProductAssets();
            GenerateUpgradeAssets();
            GenerateSupplierAndLottoAssets();
            GenerateMaterialAssets();
            GenerateIconTextures();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Kiosk: Alle Platzhalter-Assets wurden generiert.");
        }

        [MenuItem("Kiosk/Produkt-ScriptableObjects generieren")]
        public static void GenerateProductAssets()
        {
            EnsureFolder("Assets/ScriptableObjects/Products");
            foreach (var p in DefaultGameData.Products)
            {
                var copy = Object.Instantiate(p);
                copy.name = p.Id;
                CreateOrReplaceAsset(copy, "Assets/ScriptableObjects/Products/" + p.Id + ".asset");
            }
            Debug.Log("Kiosk: " + DefaultGameData.Products.Count + " Produkte exportiert.");
        }

        [MenuItem("Kiosk/Upgrade-ScriptableObjects generieren")]
        public static void GenerateUpgradeAssets()
        {
            EnsureFolder("Assets/ScriptableObjects/Upgrades");
            foreach (var u in DefaultGameData.Upgrades)
            {
                var copy = Object.Instantiate(u);
                copy.name = u.Id;
                CreateOrReplaceAsset(copy, "Assets/ScriptableObjects/Upgrades/" + u.Id + ".asset");
            }
            Debug.Log("Kiosk: " + DefaultGameData.Upgrades.Count + " Upgrades exportiert.");
        }

        [MenuItem("Kiosk/Lieferanten und Lotto generieren")]
        public static void GenerateSupplierAndLottoAssets()
        {
            EnsureFolder("Assets/ScriptableObjects/Customers");
            foreach (var s in DefaultGameData.Suppliers)
            {
                var copy = Object.Instantiate(s);
                copy.name = s.Id;
                CreateOrReplaceAsset(copy, "Assets/ScriptableObjects/Customers/" + s.Id + ".asset");
            }
            foreach (var t in DefaultGameData.LottoTickets)
            {
                var copy = Object.Instantiate(t);
                copy.name = t.Id;
                CreateOrReplaceAsset(copy, "Assets/ScriptableObjects/Customers/" + t.Id + ".asset");
            }
        }

        [MenuItem("Kiosk/Material-Assets generieren")]
        public static void GenerateMaterialAssets()
        {
            EnsureFolder("Assets/Materials");
            string[] names =
            {
                "Material_Boden", "Material_Wand", "Material_Decke", "Material_Regal",
                "Material_Kasse", "Material_Kuehlschrank", "Material_Produkt_Getraenk",
                "Material_Produkt_Snack", "Material_Produkt_Zeitung",
                "Material_Produkt_Tabak_Fiktiv", "Material_Paket",
                "Material_LottoTerminal", "Material_Kunde", "Material_Theke"
            };
            ProceduralAssetGenerator.ClearCache();
            foreach (var name in names)
            {
                var mat = new Material(ProceduralAssetGenerator.GetMaterial(name)) { name = name };
                if (mat.mainTexture is Texture2D)
                {
                    var tex = Object.Instantiate((Texture2D)mat.mainTexture);
                    tex.name = name + "_Tex";
                    string texPath = "Assets/Textures/" + tex.name + ".asset";
                    EnsureFolder("Assets/Textures");
                    CreateOrReplaceAsset(tex, texPath);
                    mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                }
                CreateOrReplaceAsset(mat, "Assets/Materials/" + name + ".mat");
            }
            Debug.Log("Kiosk: Materialien exportiert.");
        }

        [MenuItem("Kiosk/Icon-Texturen generieren")]
        public static void GenerateIconTextures()
        {
            EnsureFolder("Assets/Textures");
            string[] names =
            {
                "Texture_Icon_Geld", "Texture_Icon_Bestand", "Texture_Icon_Bestellung",
                "Texture_Icon_Upgrade", "Texture_Icon_Paket", "Texture_Icon_Lotto",
                "Texture_Icon_Alter"
            };
            foreach (var name in names)
            {
                var sprite = ProceduralAssetGenerator.GetIcon(name);
                var tex = Object.Instantiate(sprite.texture);
                tex.name = name;
                CreateOrReplaceAsset(tex, "Assets/Textures/" + name + ".asset");
            }
            Debug.Log("Kiosk: Icons exportiert.");
        }

        [MenuItem("Kiosk/Szenen neu generieren")]
        public static void RegenerateScenes()
        {
            EnsureFolder("Assets/Scenes");

            // KioskGame: ein Objekt mit SceneBootstrapper
            var gameScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrapper = new GameObject("SceneBootstrapper");
            bootstrapper.AddComponent<SceneBootstrapper>();
            EditorSceneManager.SaveScene(gameScene, "Assets/Scenes/KioskGame.unity");

            // MainMenu: ein Objekt mit MainMenuBootstrapper
            var menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var menuGo = new GameObject("MainMenuBootstrapper");
            menuGo.AddComponent<MainMenuBootstrapper>();
            EditorSceneManager.SaveScene(menuScene, "Assets/Scenes/MainMenu.unity");

            // Build Settings setzen
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/KioskGame.unity", true)
            };
            Debug.Log("Kiosk: Szenen neu generiert und in den Build Settings eingetragen.");
        }

        [MenuItem("Kiosk/Spielstand loeschen")]
        public static void DeleteSaveFile()
        {
            Kiosk.SaveSystem.SaveLoadSystem.DeleteSave();
            Debug.Log("Kiosk: Spielstand geloescht.");
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        static void CreateOrReplaceAsset(Object asset, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (existing != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
