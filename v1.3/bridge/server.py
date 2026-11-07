from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import json

ROOT = Path(__file__).resolve().parents[1]
WEB = ROOT / "web"

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
        if self.path == "/health":
            return self._send(200, '{"ok":true}', "application/json; charset=utf-8")
        path = "/index.html" if self.path == "/" else self.path.split("?", 1)[0]
        target = (WEB / path.lstrip("/")).resolve()
        if WEB.resolve() not in target.parents or not target.is_file():
            return self._send(404, "Não encontrado", "text/plain; charset=utf-8")
        types = {".html":"text/html; charset=utf-8", ".css":"text/css; charset=utf-8", ".js":"application/javascript; charset=utf-8"}
        return self._send(200, target.read_bytes(), types.get(target.suffix, "application/octet-stream"))

    def log_message(self, fmt, *args):
        print("[JVPLEY]", fmt % args)

if __name__ == "__main__":
    address = ("127.0.0.1", 8765)
    print("JVPLEY 1.3 em http://127.0.0.1:8765")
    print("Telemetria real ainda não conectada.")
    ThreadingHTTPServer(address, Handler).serve_forever()
