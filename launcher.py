import argparse
import shutil
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


def find_browser():
    """Find Edge or Chrome so the central opens in an app-style window."""
    candidates = [
        shutil.which("msedge"),
        shutil.which("chrome"),
        str(Path("C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe")),
        str(Path("C:/Program Files/Microsoft/Edge/Application/msedge.exe")),
        str(Path("C:/Program Files/Google/Chrome/Application/chrome.exe")),
        str(Path("C:/Program Files (x86)/Google/Chrome/Application/chrome.exe")),
    ]
    for candidate in candidates:
        if candidate and Path(candidate).is_file():
            return candidate
    return None


def open_central():
    browser = find_browser()
    if browser:
        try:
            subprocess.Popen([
                browser,
                "--app=" + URL,
                "--start-fullscreen",
                "--no-first-run",
                "--disable-session-crashed-bubble",
            ])
            return
        except OSError:
            pass
    # Fallback: use the Windows default browser if Edge/Chrome is not found.
    webbrowser.open(URL)


def main():
    parser = argparse.ArgumentParser(description="CARPLEY Universal 1.3")
    parser.add_argument("--demo", action="store_true", help="Abrir sem esperar pelo ETS2")
    args = parser.parse_args()

    print("CARPLEY Universal 1.3")
    if args.demo:
        open_central()
        return

    print("Aguardando o Euro Truck Simulator 2...")
    opened = False
    while True:
        running = game_is_running()
        if running and not opened:
            open_central()
            opened = True
            print("ETS2 detectado. Central CARPLEY aberta.")
        elif not running:
            opened = False
        time.sleep(3)


if __name__ == "__main__":
    main()
