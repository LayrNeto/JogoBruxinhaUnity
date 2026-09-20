using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace JogoBruxinha.Core.Analytics
{
    [Serializable]
    public sealed class PlaytestSessionData
    {
        public string sessionID;
        public float totalPlaytime;
        public bool finishedTutorial;
        public int patientsAttended;
        public int catPetted;
        public int dialogueRestarted;
        public int wrongPotionMade;
        public List<string> patientsTreatedWrong;
    }

    public sealed class PlaytestLogger : MonoBehaviour
    {
        public static PlaytestLogger Instance { get; private set; }

        private PlaytestSessionData _currentSession;
        private float _sessionStartTime;
        private string _logDirectory;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _logDirectory = Path.Combine(Application.persistentDataPath, "PlaytestLogs");
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void StartNewSession()
        {
            _sessionStartTime = Time.realtimeSinceStartup;
            
            _currentSession = new PlaytestSessionData
            {
                sessionID = DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                finishedTutorial = false,
                patientsAttended = 0,
                catPetted = 0,
                dialogueRestarted = 0,
                wrongPotionMade = 0,
                patientsTreatedWrong = new List<string>()
            };

            Debug.Log($"Iniciando gravação do Playtest: {_currentSession.sessionID}");
        }

        // Métodos para você chamar de outros scripts
        public void RecordTutorialFinished() { if (_currentSession != null) _currentSession.finishedTutorial = true; }
        public void RecordPatientAttended() { if (_currentSession != null) _currentSession.patientsAttended++; }
        public void RecordCatPetting() { if (_currentSession != null) _currentSession.catPetted++; }
        public void RecordDialogueRestarted() { if (_currentSession != null) _currentSession.dialogueRestarted++; }
        public void RecordWrongPotionMade() { if (_currentSession != null) _currentSession.wrongPotionMade++; }
        public void RecordPatientTreatedWrong(string patientName) { if (_currentSession != null) _currentSession.patientsTreatedWrong.Add(patientName); }

        public void EndAndSaveSession()
        {
            if (_currentSession == null) return;

            _currentSession.totalPlaytime = Time.realtimeSinceStartup - _sessionStartTime;

            string fileName = $"playtest_{_currentSession.sessionID}.json";
            string filePath = Path.Combine(_logDirectory, fileName);

            string json = JsonConvert.SerializeObject(_currentSession, Formatting.Indented);
            File.WriteAllText(filePath, json);

            Debug.Log($"Sessão de playtest salva em: {filePath}");
            _currentSession = null;
        }

        private void OnApplicationQuit()
        {
            EndAndSaveSession();
        }
    }
}