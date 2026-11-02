using System;

namespace JogoBruxinha.Gameplay.Inventory
{
    public enum SymptomType
    {
        None = 0,
        AntiParasita,
        Analgesico,
        Resfriante,
        Reconstruir,
        Energizante,
        AntiEnjoo,
        Clareza,
        Relaxante,
        Repelente,
        Ressecar,
        Calor,
        Sorte
    }

    [Serializable]
    public struct SymptomScoreWeight
    {
        public SymptomType symptom;
        public int points;
    }
}