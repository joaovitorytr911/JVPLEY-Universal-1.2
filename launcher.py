import argparse
import subprocess
import time
import webbrowser
from pathlib import Path

ROOT = Path(__file__).resolve().parent
URL = (ROOT / "app" / "index.html").as_uri()

def game_is_running():
    try:
        result = subprocess.run(
            ["tasklist", "/FI", "IMAGENAME eq eurotrucks2.exe"],
            capture_output=True, text=True, check=False
        )
        return "eurotrucks2.exe" in result.stdout.lower()
    except Exception:
        return False

def main():
    parser = argparse.ArgumentParser(description="JVPLEY Universal 1.2")
    parser.add_argument("--demo", action="store_true", help="Abrir sem esperar pelo ETS2")
    args = parser.parse_args()

    print("JVPLEY Universal 1.2 - Assistente SCANIA")
    if args.demo:
        webbrowser.open(URL)
        return

    print("Aguardando o Euro Truck Simulator 2...")
    opened = False
    while True:
        if game_is_running() and not opened:
            webbrowser.open(URL)
            opened = True
            print("ETS2 detectado. JVPLEY iniciado.")
        if not game_is_running():
            opened = False
        time.sleep(3)

if __name__ == "__main__":
    main()
