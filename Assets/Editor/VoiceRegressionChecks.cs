using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JogoBruxinha.Core.Audio;
using JogoBruxinha.Gameplay.Counter;
using JogoBruxinha.Gameplay.Dialogue;
using JogoBruxinha.Gameplay.Tutorial;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class VoiceRegressionChecks
{
    public static void CheckInPlayMode()
    {
        const string root = "Assets/_Project/";
        var voices = new Dictionary<string, SoundDataSO>();
        foreach (string name in new[] { "Cora", "Agata", "Aldara", "Horacio" })
        {
            var sound = AssetDatabase.LoadAssetAtPath<SoundDataSO>($"{root}ScriptableObjects/Audio/SFX/Vozes/{name}Voice.asset");
            Require(sound != null && sound.Clip != null && sound.Clip.name == $"voz_{name.ToLowerInvariant()}_teste",
                name + " has a SoundData asset with the correct WAV");
            voices.Add(name, sound);
        }
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(root + "Prefabs/Systems/GameplaySystems.prefab");
        var template = prefab.GetComponentInChildren<DialogueManager>(true);
        Require(Get<SoundDataSO>(template, "_playerVoice") == voices["Cora"], "Cora uses her sound asset");
        foreach (string name in new[] { "Aldara", "Horacio" })
            Require(AssetDatabase.LoadAssetAtPath<PatientDataSO>($"{root}ScriptableObjects/Patients/{name}.asset").voiceSound == voices[name],
                name + " patient references its sound asset");
        Require(AssetDatabase.LoadAssetAtPath<TutorialDataSO>(root + "ScriptableObjects/GameData/TutorialData.asset").agataVoice == voices["Agata"],
            "Tutorial references Agata's sound asset");

        // Tune a clone: the test never changes balancing values in project assets.
        var tunedVoice = Object.Instantiate(voices["Cora"]);
        var serialized = new SerializedObject(tunedVoice);
        serialized.FindProperty("<Volume>k__BackingField").floatValue = 0.4f;
        serialized.FindProperty("<Pitch>k__BackingField").floatValue = 1.3f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        var host = new GameObject("Voice checks");
        host.SetActive(false);
        var manager = host.AddComponent<DialogueManager>();
        var canvas = new GameObject("Dialogue", typeof(RectTransform), typeof(Canvas));
        canvas.transform.SetParent(host.transform, false);
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var image = new GameObject("Box", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        image.transform.SetParent(canvas.transform, false);
        var text = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        text.transform.SetParent(canvas.transform, false);
        text.rectTransform.sizeDelta = new Vector2(500f, 200f);
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(root + "Fonts/neodgm SDF.asset");
        Sprite box = Get<Sprite>(template, "_playerDialogueBox");
        Set(manager, "_dialogueCanvasRoot", canvas);
        Set(manager, "_nextButton", image.gameObject);
        Set(manager, "_dialogueBackgroundImage", image);
        Set(manager, "_dialogueText", text);
        Set(manager, "_playerDialogueBox", box);
        Set(manager, "_playerVoice", tunedVoice);
        Set(manager, "_voiceVolume", 0.35f);
        host.SetActive(true);
        try
        {
            var lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = SpeakerType.Player, text = "Bom dia, senhor!" },
                new DialogueLine { speaker = SpeakerType.NPC, text = "Bom dia, Cora!" }
            };
            manager.PlayDialogue(box, lines, npcVoice: voices["Horacio"]);
            manager.SendMessage("Update");
            var source = Get<AudioSource>(manager, "_voiceSource");
            Require(manager.IsTyping && source.clip == tunedVoice.Clip, "Typewriter uses the player's voice clip");
            Require(Mathf.Approximately(source.volume, 0.14f) && Mathf.Approximately(source.pitch, 1.3f),
                "Voice applies per-asset volume times global volume and per-asset pitch");
            Set(manager, "_lastAdvanceFrame", -1);
            manager.ShowNextLine();
            Require(!manager.IsTyping && !source.isPlaying, "Skipping finishes text and stops the voice");
            Set(manager, "_lastAdvanceFrame", -1);
            manager.ShowNextLine();
            manager.SendMessage("Update");
            Require(source.clip == voices["Horacio"].Clip && Mathf.Approximately(source.pitch, voices["Horacio"].Pitch),
                "Switching speakers uses the NPC clip and resets pitch");
            manager.ForceCloseDialogue();
            Require(!manager.IsPlaying && !source.isPlaying, "Closing dialogue stops voice playback");
            manager.PlayDialogue(box, new List<DialogueLine> { lines[1] });
            manager.SendMessage("Update");
            Require(manager.IsTyping && Get<SoundDataSO>(manager, "_currentVoice") == null,
                "A missing voice keeps dialogue working silently");
            manager.ForceCloseDialogue();
            Debug.Log("VOICE_CHECKS_PASSED");
        }
        finally
        {
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(tunedVoice);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        Debug.Log("VOICE_CHECK: " + message);
    }

    private static FieldInfo Field(object target, string name) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
    private static T Get<T>(object target, string name) => (T)Field(target, name).GetValue(target);
    private static void Set(object target, string name, object value) => Field(target, name).SetValue(target, value);
}
