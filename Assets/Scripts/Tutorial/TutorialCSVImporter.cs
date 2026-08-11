using UnityEngine;
using System.Collections.Generic;
using System;

public class TutorialCSVImporter : MonoBehaviour
{
    [Header("Import Settings")]
    public TextAsset csvFile; 
    public TutorialDataSO targetTutorialSO; 

    [ContextMenu("Import CSV to Tutorial")]
    public void ImportCSV()
    {
        if (csvFile == null || targetTutorialSO == null)
        {
            Debug.LogError("Faltam arquivos! Arraste o CSV e o TutorialDataSO no Inspector.");
            return;
        }

        targetTutorialSO.allDialogues = new List<TutorialDialogueBlock>();

        Dictionary<TutorialContext, List<DialogueLine>> groupedDialogues = new Dictionary<TutorialContext, List<DialogueLine>>();

        string[] rows = csvFile.text.Split('\n');

        for (int i = 1; i < rows.Length; i++)
        {
            string row = rows[i].Trim();
            if (string.IsNullOrEmpty(row)) continue;

            string[] columns = row.Split('\t');
            if (columns.Length < 3) continue;

            string contextStr = columns[0].Trim(); 
            string speakerStr = columns[1].Trim().ToLower();
            string textStr = columns[2].Trim();

            if (Enum.TryParse(contextStr, true, out TutorialContext parsedContext))
            {
                bool needsMagic = false;
                if (columns.Length >= 4) 
                {
                    string conditionStr = columns[3].Trim().ToLower();
                    if (conditionStr == "magia") needsMagic = true;
                }

                DialogueLine newLine = new DialogueLine
                {
                    text = textStr,
                    speaker = (speakerStr == "player") ? SpeakerType.Player : SpeakerType.NPC,
                    requiresMagic = needsMagic
                };

                if (!groupedDialogues.ContainsKey(parsedContext))
                {
                    groupedDialogues[parsedContext] = new List<DialogueLine>();
                }
                
                groupedDialogues[parsedContext].Add(newLine);
            }
            else
            {
                Debug.LogWarning($"Aviso na Linha {i}: O contexto '{contextStr}' não existe na enum TutorialContext. Linha ignorada.");
            }
        }

        foreach (KeyValuePair<TutorialContext, List<DialogueLine>> kvp in groupedDialogues)
        {
            TutorialDialogueBlock newBlock = new TutorialDialogueBlock
            {
                interactionID = kvp.Key,
                sequence = new DialogueSequence { lines = kvp.Value }
            };

            targetTutorialSO.allDialogues.Add(newBlock);
        }

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(targetTutorialSO);
        UnityEditor.AssetDatabase.SaveAssets();
        #endif

        Debug.Log($"CSV do Tutorial importado com sucesso para {targetTutorialSO.name}!");
    }
}