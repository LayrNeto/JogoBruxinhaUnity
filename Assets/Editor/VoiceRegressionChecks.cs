using System;
using System.Collections;
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
    public static void Run(MonoBehaviour runner, Action done, Action<Exception> fail)
    {
        runner.StartCoroutine(Guard(CheckInPlayMode(), done, fail));
    }

    private static IEnumerator Guard(IEnumerator routine, Action done, Action<Exception> fail)
    {
        using (routine as IDisposable)
        {
            while (true)
            {
                object current;
                try
                {
                    if (!routine.MoveNext()) break;
                    current = routine.Current;
                }
                catch (Exception error) { fail(error); yield break; }
                yield return current;
            }
        }
        done();
    }

    private static IEnumerator CheckInPlayMode()
    {
        const string root = "Assets/_Project/";
        var voices = new Dictionary<string, SoundDataSO>();
        foreach (string name in new[] { "Cora", "Agata", "Aldara", "Horacio" })
        {
            var sound = AssetDatabase.LoadAssetAtPath<SoundDataSO>($"{root}ScriptableObjects/Audio/SFX/Vozes/{name}Voice.asset");
            Require(sound != null && sound.Clip != null && sound.Clip.name == $"voz_{name.ToLowerInvariant()}_teste",
                name + " has a SoundData asset with the correct WAV");
            voices.Add(name, sound);
            var importer = (AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sound.Clip));
            Require(importer.defaultSampleSettings.compressionFormat == AudioCompressionFormat.PCM &&
                sound.Clip.loadType == AudioClipLoadType.DecompressOnLoad, name + " uses decompressed PCM voice audio");
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
            Require(manager.IsTyping && source.clip.name == tunedVoice.Clip.name + " (dialogue blip)",
                "Typewriter uses a prepared player syllable, preserving the original WAV");
            CheckEnvelope(source.clip, source.pitch, tunedVoice.VoiceBlipDuration);
            var playback = Get<DialogueVoicePlayback>(manager, "_voicePlayback");
            Require(!playback.TryPlay(tunedVoice, 0.35f), "Letters cannot restart or overlap an active syllable");
            Require(Mathf.Approximately(source.volume, 0.14f) && Mathf.Approximately(source.pitch, 1.3f),
                "Voice applies per-asset volume times global volume and per-asset pitch");
            Set(manager, "_lastAdvanceFrame", -1);
            manager.ShowNextLine();
            Require(!manager.IsTyping, "Skipping finishes text immediately without cutting the waveform");
            yield return new WaitForSecondsRealtime(0.2f);
            Require(!source.isPlaying, "Skipping leaves no queued voice after the short release tail");
            Set(manager, "_lastAdvanceFrame", -1);
            manager.ShowNextLine();
            manager.SendMessage("Update");
            Require(source.clip.name == voices["Horacio"].Clip.name + " (dialogue blip)" && Mathf.Approximately(source.pitch, voices["Horacio"].Pitch),
                "Switching speakers uses the NPC clip and resets pitch");
            CheckEnvelope(source.clip, source.pitch, voices["Horacio"].VoiceBlipDuration);
            manager.ForceCloseDialogue();
            yield return new WaitForSecondsRealtime(0.2f);
            Require(!manager.IsPlaying && !source.isPlaying, "Closing dialogue stops voice playback");
            manager.PlayDialogue(box, new List<DialogueLine> { lines[1] });
            manager.SendMessage("Update");
            Require(manager.IsTyping && Get<SoundDataSO>(manager, "_currentVoice") == null,
                "A missing voice keeps dialogue working silently");
            manager.ForceCloseDialogue();
            CheckPreparedVoices(voices, source);
            Debug.Log("VOICE_CHECKS_PASSED");
        }
        finally
        {
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(tunedVoice);
        }
    }

    private static void CheckEnvelope(AudioClip clip, float pitch, float maxDuration)
    {
        var samples = new float[clip.samples * clip.channels];
        Require(clip.GetData(samples, 0), "Prepared PCM samples can be inspected");
        for (int channel = 0; channel < clip.channels; channel++)
            Require(samples[channel] == 0f && samples[samples.Length - clip.channels + channel] == 0f,
                "Every channel starts and ends at silence");
        Require(samples.Any(value => Mathf.Abs(value) > 0.00001f), "Voice is not accidentally silent");
        Require(samples.All(value => !float.IsNaN(value) && Mathf.Abs(value) <= 0.981f), "Voice retains clipping headroom");
        Require(clip.length / pitch <= maxDuration + 0.0001f, "Pitch does not lengthen the skip tail");
    }

    private static void CheckPreparedVoices(Dictionary<string, SoundDataSO> voices, AudioSource source)
    {
        foreach (var voice in voices.Values)
        {
            using (var playback = new DialogueVoicePlayback(source))
            {
                Require(playback.TryPlay(voice, 0.35f), voice.name + " plays");
                CheckEnvelope(source.clip, source.pitch, voice.VoiceBlipDuration);
            }
        }

        var clone = Object.Instantiate(voices["Horacio"]);
        try
        {
            var settings = new SerializedObject(clone);
            settings.FindProperty("<VoiceGainDb>k__BackingField").floatValue = 0f;
            settings.ApplyModifiedPropertiesWithoutUndo();
            float[] reference;
            using (var playback = new DialogueVoicePlayback(source))
            {
                playback.TryPlay(clone, 0.35f);
                reference = new float[source.clip.samples * source.clip.channels];
                source.clip.GetData(reference, 0);
            }
            using (var playback = new DialogueVoicePlayback(source))
            {
                playback.TryPlay(voices["Horacio"], 0.35f);
                var boosted = new float[source.clip.samples * source.clip.channels];
                source.clip.GetData(boosted, 0);
                Require(reference.Zip(boosted, (a, b) => Mathf.Abs(b - a * 1.2f)).Max() < 0.00001f,
                    "Horacio gains exactly 20 percent amplitude without clipping");
            }
            foreach (float pitch in new[] { 0.1f, 3f })
            {
                settings.FindProperty("<Pitch>k__BackingField").floatValue = pitch;
                settings.ApplyModifiedPropertiesWithoutUndo();
                using (var playback = new DialogueVoicePlayback(source))
                {
                    playback.TryPlay(clone, 0.35f);
                    CheckEnvelope(source.clip, source.pitch, clone.VoiceBlipDuration);
                }
            }
        }
        finally { Object.DestroyImmediate(clone); }
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
