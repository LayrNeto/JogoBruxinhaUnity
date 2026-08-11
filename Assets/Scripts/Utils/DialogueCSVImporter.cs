using UnityEngine;
using System.Collections.Generic;

public class DialogueCSVImporter : MonoBehaviour
{
    [Header("Import Settings")]
    public TextAsset csvFile; 
    public PatientDataSO targetPatientSO; 


    [ContextMenu("Import CSV to Patient")]
    public void ImportCSV()
    {
        if (csvFile == null || targetPatientSO == null)
        {
            Debug.LogError("Faltam arquivos! Arraste o CSV e o patientSO no Inspector.");
            return;
        }

        targetPatientSO.introDialogue = new List<DialogueLine>();
        targetPatientSO.waitingDialogues = new List<DialogueSequence>();
        targetPatientSO.successDialogue = new List<DialogueLine>();
        targetPatientSO.failDialogue = new List<DialogueLine>();

        string[] rows = csvFile.text.Split('\n');

        for (int i = 1; i < rows.Length; i++)
        {
            string row = rows[i].Trim();
            if (string.IsNullOrEmpty(row)) continue;

            string[] columns = row.Split('\t');
            if (columns.Length < 3) continue;

            string phaseStr = columns[0].Trim().ToLower();
            string speakerStr = columns[1].Trim().ToLower();
            string textStr = columns[2].Trim();

            bool needsMagic = false;
            if (columns.Length >= 4) 
            {
                string conditionStr = columns[3].Trim().ToLower();
                if (conditionStr == "magia") 
                {
                    needsMagic = true;
                }
            }

            DialogueLine newLine = new DialogueLine
            {
                text = textStr,
                speaker = (speakerStr == "player") ? SpeakerType.Player : SpeakerType.NPC,
                requiresMagic = needsMagic
            };
            
            if (phaseStr == "intro")
            {
                targetPatientSO.introDialogue.Add(newLine);
            }
            else if (phaseStr == "success")
            {
                targetPatientSO.successDialogue.Add(newLine);
            }
            else if (phaseStr == "fail")
            {
                targetPatientSO.failDialogue.Add(newLine);
            }
            else if (phaseStr.StartsWith("waiting_"))
            {
                string numberPart = phaseStr.Replace("waiting_", "");
                if (int.TryParse(numberPart, out int sequenceNumber))
                {
                    int listIndex = sequenceNumber - 1; 

                    while (targetPatientSO.waitingDialogues.Count <= listIndex)
                    {
                        targetPatientSO.waitingDialogues.Add(new DialogueSequence { lines = new List<DialogueLine>() });
                    }

                    targetPatientSO.waitingDialogues[listIndex].lines.Add(newLine);
                }
            }
        }

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(targetPatientSO);
        UnityEditor.AssetDatabase.SaveAssets();
        #endif

        Debug.Log($"CSV Importado com sucesso para {targetPatientSO.name}!");
    }
}