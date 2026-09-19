using System;
using System.Linq;
using JogoBruxinha.Core.Audio;
using JogoBruxinha.Gameplay.Cauldron;
using JogoBruxinha.Gameplay.GameFlow;
using JogoBruxinha.Gameplay.Tutorial;
using JogoBruxinha.Gameplay.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Explicit migration tool; never runs automatically on import or in a player build.
public static class PovFeedbackSetup
{
    private const string Root = "Assets/_Project/";

    public static void ApplyAndCheckBatch()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use a dedicated batch editor.");
        var previous = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            var tutorial = Asset<TutorialDataSO>("ScriptableObjects/GameData/TutorialData.asset");
            tutorial.agataVoice = Asset<SoundDataSO>("ScriptableObjects/Audio/SFX/Vozes/AgataVoice.asset");
            EditorUtility.SetDirty(tutorial);
            var session = Asset<SessionDataSO>("ScriptableObjects/GameData/SesionData.asset");
            var scene = EditorSceneManager.OpenScene(Root + "Scenes/HouseScene.unity");
            var components = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Component>(true)).ToArray();

            var guide = components.OfType<TutorialHighlightGuide>().Single();
            var workbench = components.OfType<SpriteRenderer>().Single(sprite => sprite.name == "BalcaoCaldeiraoV1");
            Reference(guide, "_cauldronWorkbench", workbench);

            var symbol = components.OfType<Button>().Single(button => button.name == "SpellSymbol");
            var normal = Asset<Sprite>("Sprites/Interface/POV/livro/SimboloNormal.png");
            var hover = symbol.GetComponent<RitualSymbolHover>() ?? symbol.gameObject.AddComponent<RitualSymbolHover>();
            Reference(hover, "_normalSymbol", normal);
            symbol.transition = Selectable.Transition.None;
            symbol.image.sprite = normal;
            symbol.image.color = Color.white;
            symbol.GetComponent<Animator>().enabled = false;
            foreach (Canvas canvas in components.OfType<Canvas>().Where(c => c.name == "CounterCanvas" || c.name == "CauldronCanvas"))
                BuildHud(canvas.transform, session);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("POV_FEEDBACK_SETUP_PASSED");
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
            return;
        }
        if (previous.Length > 0 && previous.All(s => !string.IsNullOrEmpty(s.path)))
            EditorSceneManager.RestoreSceneManagerSetup(previous);
        else EditorSceneManager.OpenScene(Root + "Scenes/MainMenu.unity");
        PovRegressionChecks.RunBatch();
    }

    private static void BuildHud(Transform parent, SessionDataSO session)
    {
        Transform old = parent.Find("VitalEnergyHud");
        if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        // POV canvases use 426x240 logical pixels. Preserve the frame's 372:84 aspect.
        RectTransform root = Rect("VitalEnergyHud", parent, 6f, -6f, 124f, 38f);
        var label = Text("Label", root, 0f, 0f, 124f, 10f, 8f, "Energia vital");
        label.alignment = TextAlignmentOptions.TopLeft;
        RectTransform frameRect = Rect("Bar", root, 0f, -10f, 124f, 28f);
        RectTransform track = Rect("Track", frameRect, 40f / 3f, -32f / 3f, 292f / 3f, 28f / 3f);
        Image background = track.gameObject.AddComponent<Image>();
        background.color = new Color32(41, 20, 15, 255);
        background.raycastTarget = false;
        RectTransform fill = Rect("Fill", track, 0f, 0f, 0f, 0f);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.offsetMin = fill.offsetMax = Vector2.zero;
        var fillImage = fill.gameObject.AddComponent<Image>();
        fillImage.color = new Color32(218, 103, 26, 255);
        fillImage.raycastTarget = false;
        var shineRect = Rect("Shine", fill, 0f, 0f, 0f, 0f);
        shineRect.anchorMin = new Vector2(0f, 0.75f);
        shineRect.anchorMax = Vector2.one;
        shineRect.offsetMin = shineRect.offsetMax = Vector2.zero;
        var shine = shineRect.gameObject.AddComponent<Image>();
        shine.color = new Color32(255, 183, 104, 255);
        shine.raycastTarget = false;
        RectTransform overlay = Rect("Frame", frameRect, 0f, 0f, 124f, 28f);
        var frame = overlay.gameObject.AddComponent<Image>();
        frame.sprite = Asset<Sprite>("Sprites/Interface/POV/barra-de-energia_vazia.png");
        frame.preserveAspect = true;
        frame.raycastTarget = false;
        var value = Text("Value", frameRect, 40f / 3f, -32f / 3f, 292f / 3f, 28f / 3f, 6.5f, "100/100");
        value.alignment = TextAlignmentOptions.Center;
        var hud = root.gameObject.AddComponent<VitalEnergyHud>();
        Reference(hud, "_sessionData", session);
        Reference(hud, "_fill", fill);
        Reference(hud, "_value", value);
    }

    private static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = LayerMask.NameToLayer("UI");
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(w, h);
        return rect;
    }

    private static TextMeshProUGUI Text(string name, Transform parent, float x, float y, float w, float h, float size, string content)
    {
        var text = Rect(name, parent, x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = Asset<TMP_FontAsset>("Fonts/neodgm SDF.asset");
        text.fontSize = size;
        text.color = new Color32(247, 230, 201, 255);
        text.text = content;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        return text;
    }

    private static T Asset<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(Root + path);
        if (asset == null) throw new InvalidOperationException("Missing asset: " + path);
        return asset;
    }

    private static void Reference(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(field).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
