import os
import sys
import threading
import webbrowser
from pathlib import Path
from http.server import ThreadingHTTPServer

def app_root():
    if getattr(sys, "frozen", False):
        return Path(sys._MEIPASS)
    return Path(__file__).resolve().parent

ROOT = app_root()
WEB = ROOT / "web"

# Importa o servidor que já atende a interface e a API Bluetooth.
sys.path.insert(0, str(ROOT))
from bridge.server import Handler

def main():
    if not WEB.exists():
        raise RuntimeError("Pasta web não encontrada. Recrie o EXE usando CRIAR-EXE.bat.")
    server = ThreadingHTTPServer(("127.0.0.1", 8765), Handler)
    threading.Thread(target=server.serve_forever, daemon=True).start()
    webbrowser.open("http://127.0.0.1:8765")
    print("JVPLEY 1.3 iniciado. Feche esta janela para encerrar.")
    try:
        threading.Event().wait()
    except KeyboardInterrupt:
        pass
    finally:
        server.shutdown()
        server.server_close()

if __name__ == "__main__":
    main()
