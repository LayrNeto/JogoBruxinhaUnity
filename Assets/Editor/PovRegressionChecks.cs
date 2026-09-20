using System;
using System.Linq;
using System.Reflection;
using JogoBruxinha.Gameplay.Cauldron;
using JogoBruxinha.Gameplay.GameFlow;
using JogoBruxinha.Gameplay.Inventory;
using JogoBruxinha.Gameplay.SaveSystem;
using JogoBruxinha.Gameplay.Tutorial;
using JogoBruxinha.Gameplay.UI;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Runs only in a dedicated batch editor. Never saves scenes or changes inventory assets.
[InitializeOnLoad]
public static class PovRegressionChecks
{
    private const string Pending = "Bruxinha.PovRegressionChecks";
    private const string RestoreKey = Pending + ".Restore";
    [Serializable] private sealed class SceneSnapshot { public SceneSetup[] scenes; }
    static PovRegressionChecks()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false))
                EditorApplication.delayCall += CheckInPlayMode;
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Pending + ".Exiting", false))
                RestoreAndExit();
        };
    }

    public static void RunBatch()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Run in a separate batch editor only.");
        try
        {
            SessionState.SetString(RestoreKey, JsonUtility.ToJson(new SceneSnapshot { scenes = EditorSceneManager.GetSceneManagerSetup() }));
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var canvasGO = new GameObject("POV checks", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            canvasGO.SetActive(false);
            canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var inventory = ScriptableObject.CreateInstance<InventoryDataSO>();
            var session = ScriptableObject.CreateInstance<SessionDataSO>();
            session.tutorialData = null;
            session.potionAwaitingDelivery = false;

            var preview = EditorSceneManager.OpenPreviewScene("Assets/_Project/Scenes/HouseScene.unity");
            var components = preview.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true)).ToArray();
            var source = components.OfType<CauldronController>().Single();
            var guide = components.OfType<TutorialHighlightGuide>().Single();
            Require(Get<SpriteRenderer>(guide, "_cauldronWorkbench").name == "BalcaoCaldeiraoV1",
                "World highlight targets the workbench, not the pot");
            var tutorial = AssetDatabase.LoadAssetAtPath<TutorialDataSO>("Assets/_Project/ScriptableObjects/GameData/TutorialData.asset");
            Require(tutorial.agataVoice != null && tutorial.agataVoice.Clip != null &&
                tutorial.agataVoice.Clip.name == "voz_agata_teste", "Agata voice is assigned");
            var hudSource = components.OfType<VitalEnergyHud>().First();
            var hud = Object.Instantiate(hudSource.gameObject, canvasGO.transform).GetComponent<VitalEnergyHud>();
            Set(hud, "_sessionData", session);
            var cauldron = Object.Instantiate(source.gameObject, canvasGO.transform).GetComponent<CauldronController>();
            Set(cauldron, "_inventoryData", inventory);
            Set(cauldron, "_sessionData", session);
            var popup = Object.Instantiate(Get<PopupController>(source, "_popup").gameObject, canvasGO.transform).GetComponent<PopupController>();
            Set(cauldron, "_popup", popup);
            var manager = canvasGO.AddComponent<IngredientManager>();
            Set(manager, "_inventory", inventory);
            Set(cauldron, "_ingredientManager", manager);
            var symbolSource = components.OfType<Button>().Single(button => button.name == "SpellSymbol");
            var symbol = Object.Instantiate(symbolSource.gameObject, canvasGO.transform);
            symbol.name = "Symbol check";
            symbol.SetActive(true);
            symbol.GetComponent<Button>().onClick.RemoveAllListeners();
            EditorSceneManager.ClosePreviewScene(preview);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Plants/IngredientSlot.prefab");
            var slots = new IngredientSlot[3];
            string[] names = { "Babosa", "Amora", "Alecrim" };
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = Object.Instantiate(prefab, canvasGO.transform).GetComponent<IngredientSlot>();
                slots[i].name = names[i];
                var plant = AssetDatabase.LoadAssetAtPath<PlantDataSO>($"Assets/_Project/ScriptableObjects/Items/Plants/{names[i]}.asset");
                Set(slots[i], "_plantData", plant);
                inventory.savedInv[plant] = 1;
            }
            Set(manager, "_slots", slots);
            new GameObject("EventSystem", typeof(EventSystem));
            canvasGO.SetActive(true);
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Fail(error); }
    }

    private static void CheckInPlayMode()
    {
        try
        {
            var cauldron = Object.FindFirstObjectByType<CauldronController>();
            var slots = Object.FindObjectsByType<IngredientSlot>(FindObjectsSortMode.None);
            IngredientSlot first = slots.Single(slot => slot.name == "Babosa");
            IngredientSlot second = slots.Single(slot => slot.name == "Amora");
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(150, 150), pointerId = 1,
                button = PointerEventData.InputButton.Right, pointerDrag = first.gameObject
            };
            first.OnBeginDrag(pointer);
            Require(IngredientSlot.Selected == null, "Right mouse does not start a drag");
            pointer.button = PointerEventData.InputButton.Left;
            first.OnBeginDrag(pointer);
            Require(IngredientSlot.Selected == first, "Left mouse picks up the ingredient");
            pointer.position = new Vector2(310, 220);
            first.OnDrag(pointer);
            var ghost = Get<GameObject>(first, "_ghostIcon");
            Require(!ghost.GetComponent<Image>().raycastTarget && !ghost.GetComponent<CanvasGroup>().blocksRaycasts,
                "Dragged image cannot intercept the drop");
            Require(Vector2.Distance(ghost.transform.position, pointer.position) < 1f, "Dragged image follows the pointer");
            Require(cauldron.CauldronImage.raycastTarget, "Visible cauldron receives pointer events");
            cauldron.OnDrop(pointer);
            first.OnEndDrag(pointer);
            Require(cauldron.ContainsIngredient(first.PlantData) && IngredientSlot.Selected == null,
                "Drop adds the ingredient and clears the selection");
            Require(Get<Animator>(cauldron, "_cauldronAnimator").GetBool("PotionStarted"), "Drop starts the cauldron animation");
            Require(!cauldron.TryAddIngredient(first.PlantData), "Duplicate ingredient is rejected");
            pointer.pointerDrag = null;
            second.OnPointerClick(pointer);
            Require(IngredientSlot.Selected == second, "Single click picks up an ingredient");
            cauldron.OnPointerClick(pointer);
            Require(cauldron.ContainsIngredient(second.PlantData) && IngredientSlot.Selected == null,
                "Second click adds it to the cauldron");
            Require(!Get<SessionDataSO>(cauldron, "_sessionData").potionAwaitingDelivery,
                "Adding an ingredient does not also bottle a potion");

            var image = cauldron.CauldronImage;
            var animator = image.GetComponent<Animator>();
            animator.Play("cauldronCreatingPotion", 0, 0f);
            animator.Update(0f);
            Sprite frame = image.sprite;
            animator.Update(0.3f);
            Debug.Log($"POV_FRAMES first={(frame != null ? frame.name : "NULL")} next={(image.sprite != null ? image.sprite.name : "NULL")}");
            Require(frame != null && image.sprite != null && image.sprite != frame, "Cauldron animation has valid changing frames");
            image = GameObject.Find("Symbol check").GetComponent<Image>();
            animator = image.GetComponent<Animator>();
            var hover = image.GetComponent<RitualSymbolHover>();
            Sprite black = Get<Sprite>(hover, "_normalSymbol");
            Require(image.sprite == black && !animator.enabled && image.color.a > 0.9f,
                "Normal symbol is black and visible, not a paused gold frame");
            hover.OnPointerEnter(pointer);
            frame = image.sprite;
            animator.Update(0.2f);
            Debug.Log($"POV_SYMBOL_FRAMES first={(frame != null ? frame.name : "NULL")} next={(image.sprite != null ? image.sprite.name : "NULL")}");
            Require(frame != null && image.sprite != null && image.sprite != frame, "Symbol hover animation has valid changing frames");
            hover.OnPointerExit(pointer);
            Require(image.sprite == black && !animator.enabled, "Leaving symbol restores black sprite");
            hover.OnPointerEnter(pointer);
            image.gameObject.SetActive(false);
            image.gameObject.SetActive(true);
            Require(image.sprite == black && !animator.enabled, "Reopening book restores black sprite");
            image.GetComponent<Button>().interactable = false;
            hover.OnPointerEnter(pointer);
            Require(image.sprite == black && !animator.enabled, "Disabled ritual stays black on hover");
            CheckEnergy(cauldron, slots);
            VoiceRegressionChecks.Run(cauldron, () => PovPointerChecks.Run(() =>
            {
                Debug.Log("POV_CHECKS_PASSED");
                Finish(0);
            }, Fail), Fail);
        }
        catch (Exception error) { Fail(error); }
    }

    private static void CheckEnergy(CauldronController cauldron, IngredientSlot[] slots)
    {
        var session = Get<SessionDataSO>(cauldron, "_sessionData");
        var inventory = Get<InventoryDataSO>(cauldron, "_inventoryData");
        Require(session.VitalEnergy == 100, "New session starts with 100 energy");
        cauldron.BrewWithMagic();
        Require(session.VitalEnergy == 100 && !session.potionAwaitingDelivery,
            "Incomplete recipe does not spend energy");
        Require(cauldron.TryAddIngredient(slots.Single(s => s.name == "Alecrim").PlantData), "Third ingredient is accepted");
        inventory.maxPotionSlots = 0;
        cauldron.BrewWithMagic();
        Require(session.VitalEnergy == 100 && cauldron.HasEnoughIngredients && !session.potionAwaitingDelivery,
            "Full inventory does not spend energy or discard ingredients");
        inventory.maxPotionSlots = 1;
        session.RestoreVitalEnergy(50);
        cauldron.BrewWithMagic();
        Require(session.VitalEnergy == 50 && cauldron.HasEnoughIngredients && !session.potionAwaitingDelivery,
            "Insufficient energy rejects ritual without losing ingredients");
        session.RestoreVitalEnergy(100);
        cauldron.BrewWithMagic();
        Require(session.VitalEnergy == 25 && session.potionAwaitingDelivery && inventory.craftedPotions.Count == 1,
            "Successful ritual spends exactly 75 and stores a potion");
        cauldron.BrewWithMagic();
        Require(session.VitalEnergy == 25, "Repeated ritual click does not charge twice");
        var hud = Object.FindFirstObjectByType<VitalEnergyHud>();
        hud.SendMessage("LateUpdate");
        Require(Mathf.Approximately(Get<RectTransform>(hud, "_fill").anchorMax.x, 0.25f),
            "Energy bar fills one quarter at 25 energy");
        var saved = JsonConvert.DeserializeObject<GameSaveData>(JsonConvert.SerializeObject(new GameSaveData { vitalEnergy = session.VitalEnergy }));
        Require(saved.vitalEnergy == 25 && JsonConvert.DeserializeObject<GameSaveData>("{}").vitalEnergy == 100,
            "Save preserves energy and old saves default to 100");
        session.AdvanceDay();
        Require(session.VitalEnergy == 50, "New day restores 25 energy");
        session.RestoreVitalEnergy(90);
        session.AdvanceDay();
        Require(session.VitalEnergy == 100, "Daily recovery is capped at 100");
        session.RestoreVitalEnergy(25);
        session.ResetSession();
        Require(session.VitalEnergy == 100, "Reset session restores 100 energy");

        inventory.ClearData();
        foreach (var slot in slots) inventory.savedInv[slot.PlantData] = 1;
        cauldron.gameObject.SetActive(false);
        cauldron.gameObject.SetActive(true);
        session.tutorialData = ScriptableObject.CreateInstance<TutorialDataSO>();
        foreach (var slot in slots) Require(cauldron.TryAddIngredient(slot.PlantData), "Tutorial ingredient is accepted");
        cauldron.BrewWithMagic();
        Require(session.VitalEnergy == 100 && session.potionAwaitingDelivery && inventory.craftedPotions.Count == 1,
            "Tutorial ritual still creates a potion without spending energy");
        Object.Destroy(session.tutorialData);
        session.tutorialData = null;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        Debug.Log("POV_CHECK: " + message);
    }
    private static void Fail(Exception error)
    {
        Debug.LogException(error);
        Finish(1);
    }
    private static void Finish(int code)
    {
        SessionState.SetBool(Pending, false);
        SessionState.SetInt(Pending + ".ExitCode", code);
        SessionState.SetBool(Pending + ".Exiting", true);
        if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        else RestoreAndExit();
    }
    private static void RestoreAndExit()
    {
        SessionState.SetBool(Pending + ".Exiting", false);
        var snapshot = JsonUtility.FromJson<SceneSnapshot>(SessionState.GetString(RestoreKey, "{}"));
        if (snapshot?.scenes != null && snapshot.scenes.Length > 0 && snapshot.scenes.All(scene => !string.IsNullOrEmpty(scene.path)))
            EditorSceneManager.RestoreSceneManagerSetup(snapshot.scenes);
        else
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenu.unity");
        EditorApplication.Exit(SessionState.GetInt(Pending + ".ExitCode", 1));
    }
    private static T Get<T>(object instance, string name) => (T)Field(instance, name).GetValue(instance);
    private static void Set(object instance, string name, object value) => Field(instance, name).SetValue(instance, value);
    private static FieldInfo Field(object instance, string name) => instance.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
}
