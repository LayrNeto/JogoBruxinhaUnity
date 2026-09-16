using System;
using System.Collections.Generic;
using JogoBruxinha.Gameplay.Counter;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Dialogue
{
    public sealed class DialogueCSVImporter : MonoBehaviour
    {
        [Header("Import Settings")]
        [SerializeField] private TextAsset _csvFile;
        [SerializeField] private PatientDataSO _targetPatientSO;

        [ContextMenu("Import CSV to Patient")]
        public void ImportCSV()
        {
            if (_csvFile == null || _targetPatientSO == null)
            {
                Debug.LogError($"Faltam referências de arquivos em {name}! Atribua o CSV e o PatientDataSO no Inspector.", this);
                return;
            }

            _targetPatientSO.introDialogue = new List<DialogueLine>();
            _targetPatientSO.waitingDialogues = new List<DialogueSequence>();
            _targetPatientSO.successDialogue = new List<DialogueLine>();
            _targetPatientSO.failDialogue = new List<DialogueLine>();

            string[] rows = _csvFile.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

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
                    _targetPatientSO.introDialogue.Add(newLine);
                }
                else if (phaseStr == "success")
                {
                    _targetPatientSO.successDialogue.Add(newLine);
                }
                else if (phaseStr == "fail")
                {
                    _targetPatientSO.failDialogue.Add(newLine);
                }
                else if (phaseStr.StartsWith("waiting_"))
                {
                    string numberPart = phaseStr.Replace("waiting_", "");
                    if (int.TryParse(numberPart, out int sequenceNumber))
                    {
                        int listIndex = sequenceNumber - 1;

                        while (_targetPatientSO.waitingDialogues.Count <= listIndex)
                        {
                            _targetPatientSO.waitingDialogues.Add(new DialogueSequence { lines = new List<DialogueLine>() });
                        }

                        _targetPatientSO.waitingDialogues[listIndex].lines.Add(newLine);
                    }
                }
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_targetPatientSO);
            UnityEditor.AssetDatabase.SaveAssets();
#endif

            Debug.Log($"CSV Importado com sucesso para {_targetPatientSO.name}!", this);
        }
    }
}