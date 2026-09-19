using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using JogoBruxinha.Gameplay.Cauldron;
using JogoBruxinha.Gameplay.GameFlow;
using JogoBruxinha.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Exercises the actual UI input module and frame loop, not direct OnClick/OnDrag calls.
public sealed class PovPointerChecks
{
    private Mouse _mouse;
    private Mouse _otherMouse;
    private InputSystemUIInputModule _module;
    private InputSettings _originalInputSettings;
    private InputSettings _testInputSettings;
    private Action _success;
    private Action<Exception> _failure;

    public static void Run(Action success, Action<Exception> failure)
    {
        var runner = new PovPointerChecks();
        runner._success = success;
        runner._failure = failure;
        UnityEngine.Object.FindAnyObjectByType<IngredientManager>().StartCoroutine(runner.Guard(runner.Check()));
    }

    private IEnumerator Guard(IEnumerator steps)
    {
        while (true)
        {
            bool next;
            try { next = steps.MoveNext(); }
            catch (Exception error)
            {
                Cleanup();
                _failure(error);
                yield break;
            }
            if (!next) break;
            yield return steps.Current;
        }
        Cleanup();
        Debug.Log("POINTER_CHECKS_PASSED");
        _success();
    }

    private IEnumerator Check()
    {
        // A batch editor has no focused Game View. Change only a temporary settings clone.
        _originalInputSettings = InputSystem.settings;
        _testInputSettings = UnityEngine.Object.Instantiate(_originalInputSettings);
        _testInputSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        _testInputSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings = _testInputSettings;
        var cauldron = UnityEngine.Object.FindAnyObjectByType<CauldronController>();
        var slots = UnityEngine.Object.FindObjectsByType<IngredientSlot>(FindObjectsInactive.Include).OrderBy(s => s.name).ToArray();
        var session = Get<SessionDataSO>(cauldron, "_sessionData");
        var inventory = Get<InventoryDataSO>(cauldron, "_inventoryData");
        session.ResetSession();
        inventory.ClearData();
        foreach (var slot in slots)
        {
            inventory.savedInv[slot.PlantData] = 2;
            slot.gameObject.SetActive(true);
        }
        cauldron.gameObject.SetActive(false);
        cauldron.gameObject.SetActive(true);
        GameObject.Find("Symbol check").SetActive(false);
        var canvas = cauldron.GetComponentInParent<Canvas>();
        var scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(426, 240);
        scaler.referencePixelsPerUnit = 16;
        yield return null;
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < slots.Length; i++)
            Place((RectTransform)slots[i].transform, new Vector2(0.2f + i * 0.15f, 0.75f), new Vector2(30, 30));
        Place((RectTransform)cauldron.transform, new Vector2(0.72f, 0.35f), new Vector2(85, 85));
        var cauldronRect = cauldron.CauldronImage.rectTransform;
        cauldronRect.anchoredPosition = Vector2.zero;
        _mouse = InputSystem.AddDevice<Mouse>("POVTestMouse");
        _otherMouse = InputSystem.AddDevice<Mouse>("OtherMouse");
        _module = EventSystem.current.gameObject.AddComponent<InputSystemUIInputModule>();
        _module.AssignDefaultActions();
        _module.actionsAsset.Enable();
        yield return null;
        _module.ActivateModule();
        Vector2 start = RectTransformUtility.WorldToScreenPoint(null, slots[0].transform.position);
        Vector2 move = new Vector2(Screen.width * 0.5f, Screen.height * 0.55f);
        Vector2 target = RectTransformUtility.WorldToScreenPoint(null, cauldronRect.position);

        Send(start);
        yield return null;
        Send(start, left: true);
        yield return null;
        Send(start);
        yield return null;
        Require(IngredientSlot.Selected == slots[0], "Real left click selects the ingredient through UI raycasting");
        Send(move);
        yield return null;
        Follows(slots[0], move, "Selected ingredient follows movement without another click");

        // Some systems expose more than one pointer. Changing current must not freeze the held item.
        _otherMouse.MakeCurrent();
        yield return null;
        Follows(slots[0], move, "Selection stays on its own pointer when Mouse.current changes");
        Send(target);
        yield return null;
        Send(target, left: true);
        yield return null;
        Send(target);
        yield return null;
        Require(IngredientSlot.Selected == null && cauldron.ContainsIngredient(slots[0].PlantData),
            "Second click adds the selected ingredient to the cauldron");
        Require(!session.potionAwaitingDelivery, "Ingredient click does not also bottle a potion");

        start = RectTransformUtility.WorldToScreenPoint(null, slots[1].transform.position);
        Send(start);
        yield return null;
        Send(start, left: true);
        yield return null;
        Send(move, left: true);
        yield return null;
        Require(IngredientSlot.Selected == slots[1], "Held left button starts a real drag");
        Follows(slots[1], move, "Dragged ingredient follows the cursor on a scaled canvas");
        Send(target, left: true);
        yield return null;
        Send(target);
        yield return null;
        Require(IngredientSlot.Selected == null && cauldron.ContainsIngredient(slots[1].PlantData),
            "Releasing left mouse over the cauldron drops the ingredient");
        Require(Get<Animator>(cauldron, "_cauldronAnimator").GetBool("PotionStarted"), "Drop starts brewing animation");

        start = RectTransformUtility.WorldToScreenPoint(null, slots[2].transform.position);
        Send(start);
        yield return null;
        Send(start, left: true);
        yield return null;
        Send(start);
        yield return null;
        Send(move, right: true);
        yield return null;
        Require(IngredientSlot.Selected == null && !cauldron.ContainsIngredient(slots[2].PlantData),
            "Right click cancels without adding an ingredient");
        Send(start);
        yield return null;
        Send(start, left: true);
        yield return null;
        Send(start);
        yield return null;
        Require(IngredientSlot.Selected == slots[2], "Ingredient can be selected again after cancellation");
        slots[2].gameObject.SetActive(false);
        Require(IngredientSlot.Selected == null, "Closing a slot clears the carried ingredient");
    }

    private void Send(Vector2 position, bool left = false, bool right = false)
    {
        var state = new MouseState { position = position };
        if (left) state = state.WithButton(MouseButton.Left);
        if (right) state = state.WithButton(MouseButton.Right);
        InputSystem.QueueStateEvent(_mouse, state);
        InputSystem.Update();
        _module.Process();
        var hits = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, hits);
        Debug.Log($"POINTER_TRACE pos={position} mouse={_mouse.position.ReadValue()} point={_module.point.action.ReadValue<Vector2>()} left={left}/{_module.leftClick.action.ReadValue<float>()} focus={EventSystem.current.isFocused} module={EventSystem.current.currentInputModule} hits={string.Join(",", hits.Select(h => h.gameObject.name))}");
    }

    private static void Place(RectTransform rect, Vector2 anchor, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
    }

    private static void Follows(IngredientSlot slot, Vector2 position, string message)
    {
        GameObject ghost = Get<GameObject>(slot, "_ghostIcon");
        Vector2 actual = ghost != null ? RectTransformUtility.WorldToScreenPoint(null, ghost.transform.position) : Vector2.zero;
        Require(ghost != null && Vector2.Distance(actual, position) < 1f, $"{message}; expected={position}, actual={actual}");
    }

    private void Cleanup()
    {
        if (IngredientSlot.Selected != null) IngredientSlot.Selected.CancelSelection();
        if (_module != null) _module.enabled = false;
        if (_mouse != null) InputSystem.RemoveDevice(_mouse);
        if (_otherMouse != null) InputSystem.RemoveDevice(_otherMouse);
        if (_originalInputSettings != null) InputSystem.settings = _originalInputSettings;
        if (_testInputSettings != null) UnityEngine.Object.Destroy(_testInputSettings);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        Debug.Log("POINTER_CHECK: " + message);
    }

    private static T Get<T>(object target, string field) => (T)target.GetType()
        .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
}
