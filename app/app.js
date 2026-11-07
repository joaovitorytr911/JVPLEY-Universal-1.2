const assistantText=document.getElementById('assistantText');
const say=(message)=>{assistantText.textContent=message;if('speechSynthesis'in window){speechSynthesis.cancel();const u=new SpeechSynthesisUtterance(message);u.lang='pt-BR';u.rate=1;const v=speechSynthesis.getVoices().find(x=>x.lang.toLowerCase().startsWith('pt-br'));if(v)u.voice=v;speechSynthesis.speak(u);}};
const urls={spotify:'https://open.spotify.com/',youtube:'https://www.youtube.com/',maps:'https://www.google.com/maps'};
function openApp(name){if(name==='radio'){say('Rádio FM Brasil selecionada. Uma estação de streaming autorizada precisa ser configurada.');return;}say('Abrindo '+name+'.');window.open(urls[name],'_blank','noopener,noreferrer');}
document.querySelectorAll('.app').forEach(b=>b.addEventListener('click',()=>openApp(b.dataset.app)));
const Recognition=window.SpeechRecognition||window.webkitSpeechRecognition;
document.getElementById('voiceBtn').addEventListener('click',()=>{if(!Recognition){say('Reconhecimento de voz indisponível neste navegador.');return;}const r=new Recognition();r.lang='pt-BR';r.maxAlternatives=1;r.onresult=e=>{const c=e.results[0][0].transcript.toLowerCase();if(c.includes('spotify'))openApp('spotify');else if(c.includes('youtube'))openApp('youtube');else if(c.includes('rádio')||c.includes('radio'))openApp('radio');else if(c.includes('mapa'))openApp('maps');else say('Comando recebido: '+c+'.');};r.onerror=()=>say('Não consegui reconhecer. Tente novamente.');say('CARVOZ ouvindo.');r.start();});
document.getElementById('bluetoothBtn').addEventListener('click',()=>{document.getElementById('bluetoothState').textContent='Verifique o pareamento nas configurações Bluetooth do Windows ou celular.';});
document.getElementById('answerBtn').addEventListener('click',()=>say('Atender chamadas reais ainda não está integrado.'));
document.getElementById('rejectBtn').addEventListener('click',()=>say('Recusar chamadas reais ainda não está integrado.'));
function tick(){document.getElementById('clock').textContent=new Date().toLocaleTimeString('pt-BR',{hour:'2-digit',minute:'2-digit'});}tick();setInterval(tick,10000);
// Dados reais só serão exibidos quando o adaptador de telemetria enviar valores válidos.
