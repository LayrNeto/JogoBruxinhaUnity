#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using JogoBruxinha.Gameplay.Tutorial;
using JogoBruxinha.Gameplay.Counter;

public static class TutorialLocalizationBinder
{
    private const string TableCollectionName = "Gameplay_Tutorial";

    // Mapeia o enum TutorialContext diretamente para o prefixo da chave na tabela
    private static readonly Dictionary<TutorialContext, string> ContextKeyMap = new Dictionary<TutorialContext, string>
    {
        { TutorialContext.BALCAO_STEP_0, "tutorial_counter_intro" },
        { TutorialContext.BALCAO_STEP_1, "tutorial_counter_request_ingredients" },
        { TutorialContext.CALDEIRAO_INICIO_STEP_2, "tutorial_cauldron_intro" },
        { TutorialContext.CALDEIRAO_FIM_STEP_2, "tutorial_cauldron_first_potion_done" },
        { TutorialContext.BALCAO_STEP_3, "tutorial_counter_deliver_potion" },
        { TutorialContext.CALDEIRAO_INICIO_STEP_4, "tutorial_cauldron_advanced_prep" },
        { TutorialContext.CALDEIRAO_PRE_POCAO_STEP_4, "tutorial_cauldron_hint_use_spell" },
        { TutorialContext.CALDEIRAO_FIM_STEP_4, "tutorial_cauldron_magic_potion_done" },
        { TutorialContext.POS_CUTSCENE_STEP_5, "tutorial_cutscene_over" },
        { TutorialContext.CALDEIRAO_ERRO_RECEITA, "tutorial_error_wrong_recipe" },
        { TutorialContext.CALDEIRAO_ERRO_CLIQUE, "tutorial_error_spell_not_used" }
    };

    [MenuItem("Tools/Localization/Vincular Falas do Tutorial")]
    public static void BindTutorialDialogues()
    {
        string[] guids = AssetDatabase.FindAssets("t:TutorialDataSO");
        if (guids.Length == 0)
        {
            Debug.LogWarning("[Localization] Nenhum TutorialDataSO encontrado no projeto!");
            return;
        }

        int updatedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TutorialDataSO tutorial = AssetDatabase.LoadAssetAtPath<TutorialDataSO>(path);
            if (tutorial == null || tutorial.allDialogues == null) continue;

            Undo.RecordObject(tutorial, "Vincular Chaves de Localização do Tutorial");

            foreach (TutorialDialogueBlock block in tutorial.allDialogues)
            {
                if (!ContextKeyMap.TryGetValue(block.interactionID, out string baseKey))
                {
                    Debug.LogWarning($"[Localization] Contexto '{block.interactionID}' não tem mapeamento configurado no binder.", tutorial);
                    continue;
                }

                List<DialogueLine> lines = block.sequence.lines;
                if (lines == null || lines.Count == 0) continue;

                for (int i = 0; i < lines.Count; i++)
                {
                    DialogueLine line = lines[i];

                    // Gera: tutorial_error_wrong_recipe_0, tutorial_error_wrong_recipe_1, etc.
                    string key = $"{baseKey}_{i}";

                    line.text = new LocalizedString
                    {
                        TableReference = TableCollectionName,
                        TableEntryReference = key
                    };

                    lines[i] = line; // Reatribui porque DialogueLine é struct
                }
            }

            EditorUtility.SetDirty(tutorial);
            updatedCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Localization] Vinculação do Tutorial concluída! {updatedCount} asset(s) atualizados com sucesso.");
    }
}
#endif