using System;
using System.Collections.Generic;
using JogoBruxinha.Core.SceneManagement;
using JogoBruxinha.Gameplay.Cauldron;
using JogoBruxinha.Gameplay.Counter;
using JogoBruxinha.Gameplay.Dialogue;
using JogoBruxinha.Gameplay.Environment;
using JogoBruxinha.Gameplay.GameFlow;
using JogoBruxinha.Gameplay.Gardening;
using JogoBruxinha.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace JogoBruxinha.Gameplay.Tutorial
{
    // Presents the current objective without advancing or changing tutorial logic.
    public sealed class TutorialHighlightGuide : MonoBehaviour
    {
        [SerializeField] private SessionDataSO _sessionData;
        [SerializeField] private InventoryDataSO _inventoryData;
        [SerializeField] private Shader _outlineShader;
        private static readonly Color HighlightYellow = new Color(1f, 0.92f, 0.12f, 1f);
        [SerializeField, Range(1f, 4f)] private float _outlinePixels = 1.5f;
        [SerializeField, Min(0.01f)] private float _areaLineWidth = 0.06f;
        [SerializeField] private Graphic _counterExit;
        [SerializeField] private Graphic _cauldronExit;
        [Tooltip("World workbench renderer, not the pot sitting on top of it.")]
        [SerializeField] private SpriteRenderer _cauldronWorkbench;

        private sealed class Target
        {
            public TutorialHighlightVisual visual;
            public Func<bool> isCurrent;
        }

        private readonly List<Target> _targets = new List<Target>();
        private CauldronController _cauldron;
        private BookManager _book;
        private Material _spriteMaterial;
        private Material _areaMaterial;
        private int Step => _sessionData.tutorialData.tutorialStep;
        private bool IsWorld => GameStateManager.Instance != null &&
            GameStateManager.Instance.CurrentState == GameStateManager.GameState.TOP_DOWN;
        private bool IsPov => GameStateManager.Instance != null &&
            GameStateManager.Instance.CurrentState == GameStateManager.GameState.POV;
        private bool BookIsOpen => _book != null && _book.IsOpen;
        private bool IsBrewingStep => (Step == 2 || Step == 4) && !_sessionData.potionAwaitingDelivery;
        private bool NeedsRitual => Step == 4 && !_sessionData.potionAwaitingDelivery &&
            _cauldron != null && _cauldron.HasEnoughIngredients;

        private void Start()
        {
            if (_sessionData == null || _inventoryData == null || _outlineShader == null)
            {
                Debug.LogError("TutorialHighlightGuide requires session, inventory and outline shader.", this);
                enabled = false;
                return;
            }
            if (_sessionData.tutorialData == null) { enabled = false; return; }

            _spriteMaterial = new Material(_outlineShader);
            _spriteMaterial.SetColor("_OutlineColor", HighlightYellow);
            _spriteMaterial.SetFloat("_OutlinePixels", _outlinePixels);
            _areaMaterial = new Material(_spriteMaterial);
            _areaMaterial.SetFloat("_Solid", 1f);

            // Discover once, including inactive POVs, and cache the targets for this scene.
            foreach (CounterDialogueTrigger counter in SceneComponents<CounterDialogueTrigger>())
                AddWorld(counter, () => IsWorld && (Step == 0 || Step == 3));
            foreach (CauldronInteractionTrigger cauldron in SceneComponents<CauldronInteractionTrigger>())
                AddWorld(cauldron, () => IsWorld && (Step == 2 || Step == 4), _cauldronWorkbench);
            foreach (SceneTransition transition in SceneComponents<SceneTransition>())
            {
                if (transition.TargetSceneName == "EstufaScene")
                    AddArea(transition, () => IsWorld && Step == 1 && !HasIngredients());
                else if (transition.TargetSceneName == "HouseScene")
                    AddArea(transition, () => IsWorld && Step == 1 && HasIngredients());
            }
            AddGardenBeds();

            foreach (CauldronController cauldron in SceneComponents<CauldronController>()) _cauldron = cauldron;
            foreach (BookManager book in SceneComponents<BookManager>()) _book = book;
            if (_cauldron != null)
            {
                AddUI(_cauldron.CauldronImage, () => IsPov && !BookIsOpen && IsBrewingStep &&
                    (!_cauldron.HasEnoughIngredients || Step == 2), width: 1f);
                foreach (IngredientSlot slot in SceneComponents<IngredientSlot>())
                {
                    Sprite pot = slot.PlantData != null ? slot.PlantData.emptyShelfSprite : null;
                    AddUI(slot.ForegroundImg, () => IsPov && !BookIsOpen && IsBrewingStep &&
                        !_cauldron.HasEnoughIngredients && !_cauldron.ContainsIngredient(slot.PlantData),
                        pot, pot != null ? 1f : 0.55f, pot != null ? 1f : 0.6f);
                }
            }
            foreach (PatientDeliveryManager delivery in SceneComponents<PatientDeliveryManager>())
                AddUI(delivery.PotionImage, () => IsPov && Step == 3 && _sessionData.potionAwaitingDelivery);
            if (_book != null)
            {
                AddUI(GraphicOn(_book.TableBook), () => IsPov && NeedsRitual && !BookIsOpen);
                AddUI(GraphicOn(_book.NextPageButton), () => IsPov && NeedsRitual && BookIsOpen);
            }
            AddUI(_counterExit, () => IsPov && (Step == 1 || Step == 2 || Step == 4));
            AddUI(_cauldronExit, () => IsPov && Step == 3 && !BookIsOpen);
        }

        private bool HasIngredients() => _sessionData.tutorialData.HasAllIngredients(_inventoryData);
        private bool NeedsPlant(PlantDataSO plant) => plant != null &&
            _sessionData.tutorialData.requiredIngredients != null &&
            _sessionData.tutorialData.requiredIngredients.Contains(plant) &&
            (!_inventoryData.savedInv.TryGetValue(plant, out int amount) || amount < 1);
        private static Graphic GraphicOn(GameObject target) => target != null ? target.GetComponent<Graphic>() : null;

        private IEnumerable<T> SceneComponents<T>() where T : Component
        {
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
                foreach (T component in root.GetComponentsInChildren<T>(true))
                    yield return component;
        }

        private void AddGardenBeds()
        {
            var beds = new Dictionary<SpriteRenderer, List<WorldPlant>>();
            foreach (WorldPlant plant in SceneComponents<WorldPlant>())
            {
                // Bed prefabs and the greenhouse scene keep plants under the bed renderer.
                Transform parent = plant.transform.parent;
                SpriteRenderer bed = parent != null ? parent.GetComponent<SpriteRenderer>() : null;
                if (bed == null)
                {
                    Debug.LogWarning("Tutorial plant requires a parent garden bed SpriteRenderer.", plant);
                    continue;
                }
                if (!beds.TryGetValue(bed, out List<WorldPlant> plants))
                {
                    plants = new List<WorldPlant>();
                    beds.Add(bed, plants);
                }
                plants.Add(plant);
            }

            foreach (var bed in beds)
            {
                List<WorldPlant> plants = bed.Value;
                _targets.Add(new Target
                {
                    visual = TutorialHighlightVisual.ForGround(new[] { bed.Key }, _areaMaterial, _areaLineWidth),
                    isCurrent = () => IsWorld && Step == 1 && BedNeedsCollection(plants)
                });
            }
        }

        private bool BedNeedsCollection(List<WorldPlant> plants)
        {
            foreach (WorldPlant plant in plants)
                if (plant != null && plant.isActiveAndEnabled && plant.CanCollect && NeedsPlant(plant.PlantData))
                    return true;
            return false;
        }

        private void AddWorld(Component owner, Func<bool> condition, SpriteRenderer groundTarget = null)
        {
            SpriteRenderer sprite = groundTarget != null ? groundTarget : owner.GetComponent<SpriteRenderer>();
            SpriteRenderer[] sprites = sprite != null ? new[] { sprite } : owner.GetComponentsInChildren<SpriteRenderer>(true);
            if (sprites.Length == 0) { AddArea(owner, condition); return; }
            Interactable interactable = owner.GetComponent<Interactable>();
            _targets.Add(new Target
            {
                visual = TutorialHighlightVisual.ForGround(sprites, _areaMaterial, _areaLineWidth),
                isCurrent = () => condition() && (interactable == null || interactable.IsAvailable())
            });
        }

        private void AddArea(Component owner, Func<bool> condition)
        {
            BoxCollider2D area = owner.GetComponent<BoxCollider2D>();
            if (area != null) _targets.Add(new Target
            {
                visual = TutorialHighlightVisual.ForArea(area, _areaMaterial, _areaLineWidth),
                isCurrent = condition
            });
        }

        private void AddUI(Graphic graphic, Func<bool> condition, Sprite silhouette = null,
            float width = -1f, float opacity = 1f)
        {
            if (graphic != null) _targets.Add(new Target
            {
                visual = TutorialHighlightVisual.ForUI(graphic, _spriteMaterial, silhouette, width, opacity),
                isCurrent = condition
            });
        }

        private void LateUpdate()
        {
            bool canGuide = _sessionData != null && _sessionData.tutorialData != null &&
                (DialogueManager.Instance == null || !DialogueManager.Instance.IsPlaying) &&
                (FadeManager.Instance == null || !FadeManager.Instance.IsFading);
            foreach (Target target in _targets) target.visual.SetVisible(canGuide && target.isCurrent());
        }

        private void OnDisable()
        {
            foreach (Target target in _targets) target.visual.SetVisible(false, true);
        }

        private void OnDestroy()
        {
            foreach (Target target in _targets) target.visual.Dispose();
            if (_spriteMaterial != null) Destroy(_spriteMaterial);
            if (_areaMaterial != null) Destroy(_areaMaterial);
        }
    }
}
