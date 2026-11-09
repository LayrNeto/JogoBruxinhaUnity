#if UNITY_EDITOR
using System.Collections.Generic;
using JogoBruxinha.Gameplay.Characters.NPC;
using JogoBruxinha.Gameplay.Counter;
using JogoBruxinha.Gameplay.Dialogue;
using JogoBruxinha.Gameplay.Tutorial;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization;
using UnityEditor.Localization;

namespace JogoBruxinha.EditorTools
{
    public sealed class DialogueSpeakerMigrationTool : EditorWindow
    {
        [Header("Global Speaker Profiles")]
        [SerializeField] private SpeakerProfileSO _protagonistProfile;
        [SerializeField] private SpeakerProfileSO _sisterProfile;

        [MenuItem("Tools/Jogo da Bruxinha/Migrate Dialogue Speakers")]
        public static void ShowWindow()
        {
            GetWindow<DialogueSpeakerMigrationTool>("Migrate Dialogue Speakers");
        }

        private void OnGUI()
        {
            GUILayout.Label("Configuração de Locutores Fixos", EditorStyles.boldLabel);
            _protagonistProfile = (SpeakerProfileSO)EditorGUILayout.ObjectField("Protagonista (Bruxinha)", _protagonistProfile, typeof(SpeakerProfileSO), false);
            _sisterProfile = (SpeakerProfileSO)EditorGUILayout.ObjectField("Ágata / Irmã (Tutorial)", _sisterProfile, typeof(SpeakerProfileSO), false);

            EditorGUILayout.Space(10);

            if (_protagonistProfile == null || _sisterProfile == null)
            {
                EditorGUILayout.HelpBox("Atribua os perfis da Protagonista e da Ágata antes de rodar a migração.", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Migrar Diálogos do Tutorial", GUILayout.Height(32)))
            {
                MigrateTutorialAssets();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Migrar Diálogos dos Pacientes", GUILayout.Height(32)))
            {
                MigratePatientAssets();
            }
        }

        private void MigrateTutorialAssets()
        {
            string[] guids = AssetDatabase.FindAssets("t:TutorialDataSO");
            int totalLinesModified = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TutorialDataSO tutorialData = AssetDatabase.LoadAssetAtPath<TutorialDataSO>(path);

                if (tutorialData == null || tutorialData.allDialogues == null) continue;

                Undo.RecordObject(tutorialData, "Migrate Tutorial Speakers");
                bool modified = false;

                for (int b = 0; b < tutorialData.allDialogues.Count; b++)
                {
                    TutorialDialogueBlock block = tutorialData.allDialogues[b];
                    if (block.sequence.lines != null && block.sequence.lines.Count > 0)
                    {
                        List<DialogueLine> linesList = block.sequence.lines;
                        if (ProcessDialogueList(linesList, _sisterProfile, ref totalLinesModified))
                        {
                            block.sequence.lines = linesList;
                            tutorialData.allDialogues[b] = block;
                            modified = true;
                        }
                    }
                }

                if (modified)
                {
                    EditorUtility.SetDirty(tutorialData);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Migração] Tutorial concluído! {totalLinesModified} linhas associadas a perfis.");
        }

        private void MigratePatientAssets()
        {
            string[] guids = AssetDatabase.FindAssets("t:PatientDataSO");
            int totalLinesModified = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PatientDataSO patientData = AssetDatabase.LoadAssetAtPath<PatientDataSO>(path);

                if (patientData == null) continue;

                SpeakerProfileSO patientProfile = patientData.speakerProfile;
                if (patientProfile == null)
                {
                    Debug.LogWarning($"[Migração] Paciente em '{path}' não tem um 'speakerProfile' atribuído. Pulando...", patientData);
                    continue;
                }

                Undo.RecordObject(patientData, "Migrate Patient Speakers");
                bool modified = false;

                modified |= ProcessDialogueList(patientData.introDialogue, patientProfile, ref totalLinesModified);

                if (patientData.waitingDialogues != null)
                {
                    for (int w = 0; w < patientData.waitingDialogues.Count; w++)
                    {
                        DialogueSequence seq = patientData.waitingDialogues[w];
                        if (ProcessDialogueList(seq.lines, patientProfile, ref totalLinesModified))
                        {
                            patientData.waitingDialogues[w] = seq;
                            modified = true;
                        }
                    }
                }

                modified |= ProcessDialogueList(patientData.successDialogue, patientProfile, ref totalLinesModified);
                modified |= ProcessDialogueList(patientData.failDialogue, patientProfile, ref totalLinesModified);

                if (modified)
                {
                    EditorUtility.SetDirty(patientData);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Migração] Pacientes concluídos! {totalLinesModified} linhas associadas a perfis.");
        }

        private bool ProcessDialogueList(List<DialogueLine> lines, SpeakerProfileSO defaultNPCSpeaker, ref int count)
        {
            if (lines == null || lines.Count == 0) return false;
            bool modified = false;

            for (int i = 0; i < lines.Count; i++)
            {
                DialogueLine line = lines[i];
                string entryKey = ResolveEntryKey(line);

                SpeakerProfileSO resolvedSpeaker;

                // Identifica se a fala é da bruxinha / jogadora
                if (entryKey.Contains("witch") || entryKey.Contains("player") || entryKey.Contains("cora") || entryKey.Contains("protagonist"))
                {
                    resolvedSpeaker = _protagonistProfile;
                }
                // Identifica se a fala é da Ágata / Tutorial
                else if (entryKey.Contains("sister") || entryKey.Contains("agata") || entryKey.Contains("cat"))
                {
                    resolvedSpeaker = _sisterProfile;
                }
                // Identifica se é instrução pura ou narrador
                else if (entryKey.Contains("narrator") || entryKey.Contains("system"))
                {
                    resolvedSpeaker = null;
                }
                // Caso contrário, pertence ao NPC dono do arquivo (Ágata no tutorial, Paciente no balcão)
                else
                {
                    resolvedSpeaker = defaultNPCSpeaker;
                }

                if (line.speaker != resolvedSpeaker)
                {
                    line.speaker = resolvedSpeaker;
                    lines[i] = line;
                    modified = true;
                    count++;
                }
            }

            return modified;
        }

        private static string ResolveEntryKey(DialogueLine line)
        {
            if (line.text == null) return string.Empty;

            TableEntryReference entryRef = line.text.TableEntryReference;

            // 1. Se a chave foi salva como texto (Nome direto da chave)
            if (entryRef.ReferenceType == TableEntryReference.Type.Name && !string.IsNullOrEmpty(entryRef.Key))
            {
                return entryRef.Key.ToLowerInvariant();
            }

            // 2. Se a chave foi salva internamente pelo Id numérico (KeyId)
            if (entryRef.ReferenceType == TableEntryReference.Type.Id && line.text.TableReference.ReferenceType != TableReference.Type.Empty)
            {
                StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(line.text.TableReference);
                if (collection != null && collection.SharedData != null)
                {
                    SharedTableData.SharedTableEntry entry = collection.SharedData.GetEntry(entryRef.KeyId);
                    if (entry != null && !string.IsNullOrEmpty(entry.Key))
                    {
                        return entry.Key.ToLowerInvariant();
                    }
                }
            }

            return string.Empty;
        }
    }
}
#endif