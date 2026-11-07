from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import json
import os
import platform
import subprocess

ROOT = Path(__file__).resolve().parents[1]
WEB = ROOT / "web"

def bluetooth_status():
    if platform.system() != "Windows":
        return {"supported": False, "state": "unsupported", "message": "A leitura do Bluetooth está preparada para Windows 11.", "devices": []}
    script = r"""
    $items = @(Get-PnpDevice -Class Bluetooth -PresentOnly -ErrorAction SilentlyContinue |
      Where-Object { $_.FriendlyName -and $_.FriendlyName -notmatch 'Bluetooth Radio|Enumerator|RFCOMM|Protocol|Microsoft Bluetooth' } |
      Select-Object -Property FriendlyName, Status)
    ConvertTo-Json -InputObject $items -Compress
    """
    try:
        result = subprocess.run(
            ["powershell.exe", "-NoProfile", "-ExecutionPolicy", "Bypass", "-Command", script],
            capture_output=True, text=True, timeout=8
        )
        if result.returncode != 0:
            return {"supported": True, "state": "unavailable", "message": "Não foi possível consultar os dispositivos. Verifique o Bluetooth e o driver do Windows.", "devices": []}
        raw = result.stdout.strip()
        devices = json.loads(raw) if raw else []
        if isinstance(devices, dict):
            devices = [devices]
        return {
            "supported": True,
            "state": "listed" if devices else "none",
            "message": "Dispositivos Bluetooth detectados pelo Windows. A conexão ativa de áudio ainda precisa ser confirmada pelo sistema.",
            "devices": [{"name": d.get("FriendlyName", "Dispositivo Bluetooth"), "status": d.get("Status", "Unknown")} for d in devices]
        }
    except (subprocess.TimeoutExpired, json.JSONDecodeError, OSError):
        return {"supported": True, "state": "unavailable", "message": "Consulta Bluetooth indisponível. Abra as configurações do Windows para parear o celular.", "devices": []}

class Handler(BaseHTTPRequestHandler):
    def _send(self, status, body, content_type):
        data = body if isinstance(body, bytes) else body.encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", content_type)
        self.send_header("Content-Length", str(len(data)))
        self.send_header("Cache-Control", "no-store")
        self.end_headers()
        self.wfile.write(data)

    def do_GET(self):
        if self.path == "/api/status":
            payload = {
                "ok": True,
                "telemetry_connected": False,
                "telemetry": {
                    "truck": "AGUARDANDO TELEMETRIA",
                    "game_state": "Plugin SCS ainda não conectado",
                    "speed": None, "rpm": None, "gear": None,
                    "fuel": None, "consumption": None,
                    "temperature": None, "tires": None
                }
            }
            return self._send(200, json.dumps(payload), "application/json; charset=utf-8")
        if self.path == "/api/bluetooth":
            return self._send(200, json.dumps(bluetooth_status(), ensure_ascii=False), "application/json; charset=utf-8")
        if self.path == "/health":
            return self._send(200, '{"ok":true}', "application/json; charset=utf-8")
        path = "/index.html" if self.path == "/" else self.path.split("?", 1)[0]
        target = (WEB / path.lstrip("/")).resolve()
        if WEB.resolve() not in target.parents or not target.is_file():
            return self._send(404, "Não encontrado", "text/plain; charset=utf-8")
        types = {".html":"text/html; charset=utf-8", ".css":"text/css; charset=utf-8", ".js":"application/javascript; charset=utf-8"}
        return self._send(200, target.read_bytes(), types.get(target.suffix, "application/octet-stream"))

    def do_POST(self):
        if self.path == "/api/bluetooth/settings":
            if platform.system() != "Windows":
                return self._send(400, '{"ok":false,"message":"Recurso disponível no Windows 11."}', "application/json; charset=utf-8")
            try:
                os.startfile("ms-settings:bluetooth")
                return self._send(200, '{"ok":true,"message":"Configurações de Bluetooth abertas."}', "application/json; charset=utf-8")
            except OSError:
                return self._send(500, '{"ok":false,"message":"Não foi possível abrir as configurações do Windows."}', "application/json; charset=utf-8")
        return self._send(404, '{"ok":false}', "application/json; charset=utf-8")

    def log_message(self, fmt, *args):
        print("[JVPLEY]", fmt % args)

if __name__ == "__main__":
    address = ("127.0.0.1", 8765)
    print("JVPLEY 1.3 em http://127.0.0.1:8765")
    print("Telemetria real ainda não conectada.")
    print("Bluetooth: consulta de dispositivos do Windows habilitada.")
    ThreadingHTTPServer(address, Handler).serve_forever()
