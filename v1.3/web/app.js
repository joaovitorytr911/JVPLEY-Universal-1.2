const $ = (id) => document.getElementById(id);
function showTelemetry(data) {
  const fields = {speed:"speed",rpm:"rpm",gear:"gear",fuel:"fuel",consumption:"consumption",temperature:"temperature",tires:"tires"};
  for (const [key,id] of Object.entries(fields)) {
    const value = data && data[key];
    $(id).textContent = value === null || value === undefined ? "--" : value;
  }
  if (data && data.truck) $("truck").textContent = data.truck;
  if (data && data.game_state) $("gameState").textContent = data.game_state;
}
async function refresh() {
  try {
    const r = await fetch("/api/status", {cache:"no-store"});
    if (!r.ok) throw new Error("HTTP " + r.status);
    const status = await r.json();
    $("dot").style.background = "#36cf88";
    $("connection").textContent = "PONTE LOCAL: ONLINE";
    showTelemetry(status.telemetry || {});
    if (!status.telemetry_connected) $("gameState").textContent = "Servidor online; aguardando plugin de telemetria do ETS2.";
  } catch (_) {
    $("dot").style.background = "#d99b39";
    $("connection").textContent = "PONTE LOCAL: OFFLINE";
    $("gameState").textContent = "Inicie o servidor local pelo arquivo INICIAR-JVPLEY-1.3.bat.";
    showTelemetry({});
  }
}
refresh();
setInterval(refresh, 1000);