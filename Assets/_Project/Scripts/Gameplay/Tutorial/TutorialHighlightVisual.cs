using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace JogoBruxinha.Gameplay.Tutorial
{
    // Owns only the added visuals; original materials and colliders remain untouched.
    internal sealed class TutorialHighlightVisual
    {
        private const float FadeSeconds = 0.3f;
        private const int SparkCount = 5;
        private SpriteRenderer[] _groundSources;
        private Rect _groundRect;
        private bool IsGround => _groundSources != null;
        private Sprite _silhouette;
        private float _intensity = 1f;
        private Sprite _lastSprite;
        private MaterialPropertyBlock _properties;
        private BoxCollider2D _area;
        private LineRenderer _line;
        private Graphic _graphic;
        private Image _uiOutline;
        private Material _uiMaterial, _sparkMaterial;
        private GameObject _visualObject;
        private float _opacity, _emissionTimer;
        private int _emissionIndex;
        private Spark[] _sparks;

        private sealed class Spark
        {
            public Transform transform;
            public Image image;
            public LineRenderer line;
            public Vector3 origin;
            public float age = 1f;
            public float drift;
        }

        public static TutorialHighlightVisual ForGround(SpriteRenderer[] sources, Material material, float width)
        {
            var visual = new TutorialHighlightVisual { _groundSources = sources, _sparkMaterial = material };
            // A scene root avoids inheriting the furniture's SortingGroup (above the player).
            visual._visualObject = new GameObject("Tutorial ground outline");
            visual._visualObject.layer = sources[0].gameObject.layer;
            SceneManager.MoveGameObjectToScene(visual._visualObject, sources[0].gameObject.scene);
            visual.CreateLine(material, width);
            visual._line.sortingLayerName = "Map";
            visual._line.sortingOrder = 100;
            visual._properties = new MaterialPropertyBlock();
            return visual;
        }

        public static TutorialHighlightVisual ForArea(BoxCollider2D area, Material material, float width)
        {
            var visual = new TutorialHighlightVisual { _area = area, _sparkMaterial = material };
            visual.CreateObject("Tutorial transition outline", area.transform);
            visual.CreateLine(material, width);
            visual._line.sortingLayerName = "Entities";
            visual._line.sortingOrder = 100;
            visual._properties = new MaterialPropertyBlock();
            return visual;
        }

        private void CreateLine(Material material, float width)
        {
            _line = _visualObject.AddComponent<LineRenderer>();
            _line.sharedMaterial = material;
            _line.useWorldSpace = false;
            _line.loop = true;
            _line.positionCount = 4;
            _line.widthMultiplier = width;
            _line.numCornerVertices = 3;
            _line.enabled = false;
        }

        public static TutorialHighlightVisual ForUI(Graphic graphic, Material material, Sprite silhouette = null,
            float width = -1f, float opacity = 1f)
        {
            var visual = new TutorialHighlightVisual { _graphic = graphic, _silhouette = silhouette, _intensity = opacity };
            visual._visualObject = new GameObject("Tutorial yellow UI outline", typeof(RectTransform), typeof(LayoutElement));
            visual._visualObject.layer = graphic.gameObject.layer;
            visual._visualObject.transform.SetParent(graphic.transform, false);
            visual._visualObject.GetComponent<LayoutElement>().ignoreLayout = true;
            var rect = (RectTransform)visual._visualObject.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.pivot = graphic.rectTransform.pivot;
            // Sample only alpha: standard UI Outline multiplies its color by the artwork RGB.
            visual._uiMaterial = new Material(material);
            if (width > 0f) visual._uiMaterial.SetFloat("_OutlinePixels", width);
            visual._uiOutline = visual._visualObject.AddComponent<Image>();
            visual._uiOutline.material = visual._uiMaterial;
            visual._uiOutline.raycastTarget = false;
            visual._uiOutline.enabled = false;
            return visual;
        }

        private void CreateObject(string name, Transform parent)
        {
            _visualObject = new GameObject(name);
            _visualObject.layer = parent.gameObject.layer;
            _visualObject.transform.SetParent(parent, false);
        }

        public void SetVisible(bool visible, bool immediate = false)
        {
            // Scene unload can destroy a target and its children before the guide is disabled.
            if (_visualObject == null) return;
            bool available = IsGround ? UpdateGroundBounds() :
                _area != null ? _area.enabled && _area.gameObject.activeInHierarchy :
                _graphic is Image image && image.enabled && image.gameObject.activeInHierarchy &&
                    image.overrideSprite != null && image.color.a > 0f;
            visible &= available;
            _opacity = immediate || !available ? (visible ? 1f : 0f) :
                Mathf.MoveTowards(_opacity, visible ? 1f : 0f, Time.unscaledDeltaTime / FadeSeconds);
            float alpha = Mathf.SmoothStep(0f, 1f, _opacity) * (0.9f + 0.1f * Mathf.Sin(Time.unscaledTime * 2.5f)) * _intensity;
            bool show = alpha > 0.001f;
            if (_line != null)
            {
                _line.enabled = show;
                if (show)
                {
                    Rect bounds = LocalBounds();
                    Vector2 half = bounds.size * 0.5f;
                    Vector2 center = bounds.center;
                    _line.SetPosition(0, center + new Vector2(-half.x, -half.y));
                    _line.SetPosition(1, center + new Vector2(-half.x, half.y));
                    _line.SetPosition(2, center + new Vector2(half.x, half.y));
                    _line.SetPosition(3, center + new Vector2(half.x, -half.y));
                    _properties.SetFloat("_Fade", alpha);
                    _line.SetPropertyBlock(_properties);
                }
            }
            else if (_uiOutline != null)
            {
                _uiOutline.enabled = show;
                if (show) UpdateUI(alpha);
            }
            UpdateSparkles(visible && !immediate, alpha);
        }

        private bool UpdateGroundBounds()
        {
            bool found = false;
            Bounds bounds = default;
            foreach (SpriteRenderer source in _groundSources)
            {
                if (source == null || !source.enabled || !source.gameObject.activeInHierarchy || source.sprite == null) continue;
                if (!found) bounds = source.bounds;
                else bounds.Encapsulate(source.bounds);
                found = true;
            }
            if (!found) return false;
            _visualObject.transform.position = bounds.center;
            // Keep the border just outside the furniture so it remains visible on the floor.
            Vector2 size = (Vector2)bounds.size + Vector2.one * 0.16f;
            _groundRect = new Rect(-size * 0.5f, size);
            return true;
        }

        private void UpdateUI(float alpha)
        {
            if (_graphic is Image source)
            {
                _uiOutline.sprite = _silhouette != null ? _silhouette : source.overrideSprite;
                _uiOutline.type = source.type;
                _uiOutline.preserveAspect = source.preserveAspect;
                _uiOutline.fillAmount = source.fillAmount;
                _uiOutline.fillMethod = source.fillMethod;
                _uiOutline.fillOrigin = source.fillOrigin;
                _uiOutline.fillClockwise = source.fillClockwise;
                _uiOutline.fillCenter = source.fillCenter;
                _uiOutline.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
                if (_lastSprite != _uiOutline.sprite)
                {
                    _lastSprite = _uiOutline.sprite;
                    _uiMaterial.SetVector("_UVRect", _lastSprite != null ?
                        UnityEngine.Sprites.DataUtility.GetOuterUV(_lastSprite) : new Vector4(0, 0, 1, 1));
                    _uiOutline.SetMaterialDirty();
                }
            }
            _uiOutline.color = new Color(1f, 1f, 1f, alpha);
        }

        private void UpdateSparkles(bool emit, float alpha)
        {
            if (_sparks == null && !emit) return;
            if (_sparks == null) CreateSparkles();
            _emissionTimer -= Time.unscaledDeltaTime;
            if (emit && _emissionTimer <= 0f)
            {
                _emissionTimer = IsGround ? 0.1f : 0.24f;
                Spark spark = _sparks[_emissionIndex % _sparks.Length];
                // Do not alter Unity's random state used by gameplay.
                float t = Mathf.Repeat(++_emissionIndex * 0.618034f, 1f);
                Rect bounds = LocalBounds();
                float edge = t * 4f;
                spark.origin = edge < 1f ? new Vector2(Mathf.Lerp(bounds.xMin, bounds.xMax, edge), bounds.yMax) :
                    edge < 2f ? new Vector2(bounds.xMax, Mathf.Lerp(bounds.yMax, bounds.yMin, edge - 1f)) :
                    edge < 3f ? new Vector2(Mathf.Lerp(bounds.xMax, bounds.xMin, edge - 2f), bounds.yMin) :
                    new Vector2(bounds.xMin, Mathf.Lerp(bounds.yMin, bounds.yMax, edge - 3f));
                spark.age = 0f;
                spark.drift = (t - 0.5f) * 2f;
            }
            foreach (Spark spark in _sparks)
            {
                if (spark.transform == null) continue;
                spark.age += Time.unscaledDeltaTime / 0.9f;
                if (alpha <= 0.001f) spark.age = 1f;
                bool alive = spark.age < 1f;
                spark.transform.gameObject.SetActive(alive);
                if (!alive) continue;
                float distance = _graphic != null ? 12f : IsGround ? 0.35f : 0.18f;
                spark.transform.localPosition = spark.origin + new Vector3(spark.drift * distance * spark.age,
                    distance * spark.age, 0f);
                float strength = Mathf.Sin(spark.age * Mathf.PI) * alpha * (IsGround ? 1f : 0.75f);
                if (spark.image != null) spark.image.color = new Color(1f, 0.92f, 0.12f, strength);
                else if (spark.line != null && _line != null)
                {
                    spark.line.startColor = spark.line.endColor = new Color(1f, 1f, 1f, strength);
                    spark.line.sortingLayerID = _line.sortingLayerID;
                    spark.line.sortingOrder = _line.sortingOrder + 1;
                }
            }
        }

        private Rect LocalBounds()
        {
            if (_graphic != null) return _graphic.rectTransform.rect;
            if (IsGround) return _groundRect;
            return new Rect(_area.offset - _area.size * 0.5f, _area.size);
        }

        private void CreateSparkles()
        {
            _sparks = new Spark[IsGround ? 10 : SparkCount];
            for (int i = 0; i < _sparks.Length; i++)
            {
                var obj = new GameObject("Tutorial sparkle", typeof(RectTransform));
                obj.layer = _visualObject.layer;
                obj.transform.SetParent(_visualObject.transform, false);
                var spark = new Spark { transform = obj.transform };
                if (_graphic != null)
                {
                    var rect = (RectTransform)obj.transform;
                    rect.anchorMin = rect.anchorMax = _graphic.rectTransform.pivot;
                    rect.sizeDelta = new Vector2(2f, 2f);
                    spark.image = obj.AddComponent<Image>();
                    spark.image.raycastTarget = false;
                }
                else
                {
                    spark.line = obj.AddComponent<LineRenderer>();
                    spark.line.sharedMaterial = _sparkMaterial;
                    spark.line.useWorldSpace = false;
                    spark.line.positionCount = 2;
                    float size = IsGround ? 0.07f : 0.024f;
                    spark.line.SetPosition(0, new Vector3(-size * 0.5f, 0f, 0f));
                    spark.line.SetPosition(1, new Vector3(size * 0.5f, 0f, 0f));
                    spark.line.widthMultiplier = size;
                }
                obj.SetActive(false);
                _sparks[i] = spark;
            }
        }

        public void Dispose()
        {
            if (_visualObject != null) Object.Destroy(_visualObject);
            if (_uiMaterial != null) Object.Destroy(_uiMaterial);
        }
    }
}
