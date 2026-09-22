import os 
import json
import csv

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
LOGS_DIR = os.path.join(BASE_DIR, "logs")
OUTPUT_CSV = os.path.join(BASE_DIR, "relatorio_playtests.csv")

def process_logs():
    
    if not os.path.exists(LOGS_DIR):
        print(f"Pasta {LOGS_DIR} não encontrada")
        return
    
    json_files = [f for f in os.listdir(LOGS_DIR) if f.endswith(".json")]
    
    if not json_files:
        print(f"Nenhum arquivo .json encontrado em {LOGS_DIR}")
        return
    
    records = []
        
    for file_name in json_files:
        file_path = os.path.join(LOGS_DIR, file_name)
        
        try:
            with open(file_path, "r", encoding="utf-8") as f:
                data = json.load(f)
                
                patients_treated_wrong_str = "; ".join(data.get("patientsTreatedWrong", []))
                
                records.append({
                    "sessionID": data.get("sessionID", ""),
                    "totalPlaytime_sec": round(data.get("totalPlaytime", 0.0), 2),
                    "totalPlaytime_min": round(data.get("totalPlaytime", 0.0) / 60, 2),
                    "finishedTutorial": data.get("finishedTutorial", False),
                    "patientsAttended": data.get("patientsAttended", 0),
                    "catPetted": data.get("catPetted", 0),
                    "dialogueRestarted": data.get("dialogueRestarted", 0),
                    "wrongPotionMade": data.get("wrongPotionMade", 0),
                    "patientsTreatedWrong": patients_treated_wrong_str
                })
        except Exception as e:
            print(f"Erro ao ler {file_name}: {e}")
            
    if not records:
        print("Nenhum dado valido extraido.")
        return

    fieldnames = list(records[0].keys())
    
    with open(OUTPUT_CSV, "w", newline="", encoding="utf-8-sig") as csv_file:
        writer = csv.DictWriter(csv_file, fieldnames=fieldnames, delimiter=";")
        writer.writeheader()
        writer.writerows(records)

    print(f"Processamento concluido! {len(records)} sessoes salvas em '{OUTPUT_CSV}'.")

if __name__ == "__main__":
    process_logs()