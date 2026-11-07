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
async function refreshBluetooth() {
  const list = $("bluetoothDevices");
  try {
    const r = await fetch("/api/bluetooth", {cache:"no-store"});
    if (!r.ok) throw new Error("HTTP " + r.status);
    const data = await r.json();
    list.replaceChildren();
    if (data.devices && data.devices.length) {
      for (const device of data.devices) {
        const li = document.createElement("li");
        li.textContent = device.name + " — " + device.status;
        list.appendChild(li);
      }
      $("device").textContent = "DISPOSITIVOS BLUETOOTH DETECTADOS";
    } else {
      const li = document.createElement("li");
      li.textContent = "Nenhum dispositivo detectado pelo Windows.";
      list.appendChild(li);
      $("device").textContent = "NENHUM DISPOSITIVO CONECTADO";
    }
    $("bluetoothMessage").textContent = data.message || "Consulta concluída.";
  } catch (_) {
    list.innerHTML = "<li>Não foi possível consultar o Bluetooth.</li>";
    $("bluetoothMessage").textContent = "Confira se o Bluetooth está ligado e se o servidor JVPLEY está aberto.";
  }
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
$("openBluetooth").addEventListener("click", async () => {
  try {
    const r = await fetch("/api/bluetooth/settings", {method:"POST"});
    const data = await r.json();
    $("bluetoothMessage").textContent = data.message;
  } catch (_) {
    $("bluetoothMessage").textContent = "Não foi possível abrir as configurações. Use Configurações > Bluetooth e dispositivos no Windows.";
  }
});
$("refreshBluetooth").addEventListener("click", refreshBluetooth);
refresh();
refreshBluetooth();
setInterval(refresh, 1000);
setInterval(refreshBluetooth, 10000);