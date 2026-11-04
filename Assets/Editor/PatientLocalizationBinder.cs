#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using JogoBruxinha.Gameplay.Counter;

public static class PatientLocalizationBinder
{
    private const string TableCollectionName = "Gameplay_Patients";

    [MenuItem("Tools/Localization/Vincular Falas dos Pacientes")]
    public static void BindAllPatients()
    {
        // Encontra todos os ScriptableObjects de PatientData no projeto
        string[] guids = AssetDatabase.FindAssets("t:PatientDataSO");
        int updatedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PatientDataSO patient = AssetDatabase.LoadAssetAtPath<PatientDataSO>(path);

            if (patient == null) continue;

            // Se o patientId estiver vazio, usa o próprio nome do arquivo como fallback (ex: "Aldara" -> "aldara")
            string id = string.IsNullOrEmpty(patient.patientId) ? patient.name.ToLowerInvariant() : patient.patientId.ToLowerInvariant();

            Undo.RecordObject(patient, "Vincular Chaves de Localização");

            // 1. Vincula o nome do paciente
            patient.patientName = CreateLocalizedString($"patient_{id}_name");

            // 2. Vincula a lista de Intro
            BindDialogueList(patient.introDialogue, id, "intro");

            // 3. Vincula as sequências de espera (Waiting Dialogues)
            if (patient.waitingDialogues != null)
            {
                for (int s = 0; s < patient.waitingDialogues.Count; s++)
                {
                    var sequence = patient.waitingDialogues[s];
                    BindDialogueList(sequence.lines, id, $"waiting");
                }
            }

            // 4. Vincula Sucesso e Falha
            BindDialogueList(patient.successDialogue, id, "success");
            BindDialogueList(patient.failDialogue, id, "fail");

            EditorUtility.SetDirty(patient);
            updatedCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Localization] Vinculação concluída! {updatedCount} pacientes atualizados com sucesso.");
    }

    private static void BindDialogueList(System.Collections.Generic.List<DialogueLine> lines, string patientId, string context)
    {
        if (lines == null) return;

        for (int i = 0; i < lines.Count; i++)
        {
            DialogueLine line = lines[i];

            // Monta a chave respeitando o modificador de magia se houver
            string key = line.requiresMagic
                ? $"dialogue_{patientId}_{context}_{i}_magic"
                : $"dialogue_{patientId}_{context}_{i}";

            line.text = CreateLocalizedString(key);
            lines[i] = line; // Como DialogueLine é struct, precisa reatribuir no índice
        }
    }

    private static LocalizedString CreateLocalizedString(string key)
    {
        return new LocalizedString
        {
            TableReference = TableCollectionName,
            TableEntryReference = key
        };
    }
}
#endif