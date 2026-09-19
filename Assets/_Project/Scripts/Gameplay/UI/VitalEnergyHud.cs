using JogoBruxinha.Gameplay.GameFlow;
using TMPro;
using UnityEngine;

namespace JogoBruxinha.Gameplay.UI
{
    public sealed class VitalEnergyHud : MonoBehaviour
    {
        [SerializeField] private SessionDataSO _sessionData;
        [SerializeField] private RectTransform _fill;
        [SerializeField] private TMP_Text _value;
        private int _lastEnergy = -1;

        private void OnEnable()
        {
            _lastEnergy = -1;
            Refresh();
        }

        private void LateUpdate() => Refresh();

        private void Refresh()
        {
            if (_sessionData == null || _fill == null || _value == null) return;
            int energy = _sessionData.VitalEnergy;
            if (energy == _lastEnergy) return;
            _lastEnergy = energy;
            _fill.anchorMax = new Vector2(energy / (float)SessionDataSO.MaxVitalEnergy, 1f);
            _value.SetText("{0}/100", energy);
        }
    }
}
