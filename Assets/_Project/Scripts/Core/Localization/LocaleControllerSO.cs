using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace JogoBruxinha.Core.Localization
{
    [CreateAssetMenu(fileName = "LocaleController", menuName = "Scriptable Objects/Settings/Locale Controller")]
    public sealed class LocaleControllerSO : ScriptableObject
    {
        private const string PREFS_LOCALE_KEY = "selected_locale_code";
        private bool _isChanging;

        public List<Locale> AvailableLocales => LocalizationSettings.AvailableLocales.Locales;
        public Locale CurrentLocale => LocalizationSettings.SelectedLocale;

        /// <summary>
        /// Troca o idioma pelo código de cultura (ex: "pt-BR", "en", "es").
        /// Ideal para os 2 botões atuais.
        /// </summary>
        public void SetLocaleByCode(string localeCode)
        {
            if (_isChanging) return;

            Locale targetLocale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(localeCode));
            if (targetLocale != null)
            {
                ApplyLocale(targetLocale);
            }
            else
            {
                Debug.LogWarning($"[LocaleController] Idioma com código '{localeCode}' não encontrado!");
            }
        }

        /// <summary>
        /// Troca diretamente pelo índice da lista.
        /// Perfeito para o futuro Dropdown (dropdown.onValueChanged passa um int).
        /// </summary>
        public void SetLocaleByIndex(int index)
        {
            if (_isChanging) return;

            var locales = AvailableLocales;
            if (index >= 0 && index < locales.Count)
            {
                ApplyLocale(locales[index]);
            }
        }

        /// <summary>
        /// Percorre circularmente para o próximo idioma disponível.
        /// Perfeito para o futuro botão único de alternância (ex: PT -> EN -> ES -> PT).
        /// </summary>
        public void CycleNextLocale()
        {
            if (_isChanging) return;

            var locales = AvailableLocales;
            if (locales.Count == 0) return;

            int currentIndex = locales.IndexOf(CurrentLocale);
            int nextIndex = (currentIndex + 1) % locales.Count;
            ApplyLocale(locales[nextIndex]);
        }

        private void ApplyLocale(Locale targetLocale)
        {
            if (LocalizationSettings.SelectedLocale == targetLocale) return;

            LocalizationSettings.SelectedLocale = targetLocale;
            PlayerPrefs.SetString(PREFS_LOCALE_KEY, targetLocale.Identifier.Code);
            PlayerPrefs.Save();
        }
    }
}