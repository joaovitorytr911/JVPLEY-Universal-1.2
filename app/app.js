const buttons=[...document.querySelectorAll('.app')];
let selected=0;
const assistantText=document.getElementById('assistantText');
function say(message){
  assistantText.textContent=message;
  if('speechSynthesis' in window){
    speechSynthesis.cancel();
    const u=new SpeechSynthesisUtterance(message);u.lang='pt-BR';u.rate=1;u.pitch=1.08;
    const voices=speechSynthesis.getVoices();
    const pt=voices.find(v=>v.lang.toLowerCase().startsWith('pt-br'));
    if(pt)u.voice=pt;speechSynthesis.speak(u);
  }
}
function openApp(name){
  const urls={youtube:'https://www.youtube.com/',spotify:'https://open.spotify.com/',maps:'https://www.google.com/maps'};
  if(name==='radio'){say('Rádio BR FM selecionada. Configure uma estação de streaming autorizada no arquivo app.js.');return;}
  if(urls[name]){say('Abrindo '+name+'.');window.open(urls[name],'_blank','noopener');}
}
function choose(i){selected=(i+buttons.length)%buttons.length;buttons.forEach((b,n)=>b.classList.toggle('selected',n===selected));}
buttons.forEach((b,i)=>b.addEventListener('click',()=>{choose(i);openApp(b.dataset.app)}));
document.addEventListener('keydown',e=>{
 if(e.key==='F1'){e.preventDefault();say('Menu principal do JVPLEY.');}
 if(e.key==='F2')openApp('youtube');if(e.key==='F3')openApp('spotify');
 if(e.key==='F4')openApp('radio');if(e.key==='F5')openApp('maps');
 if(e.key==='ArrowDown'||e.key==='ArrowRight'){e.preventDefault();choose(selected+1);}
 if(e.key==='ArrowUp'||e.key==='ArrowLeft'){e.preventDefault();choose(selected-1);}
 if(e.key==='Enter')openApp(buttons[selected].dataset.app);
 if(e.key==='Escape')say('Voltando ao menu principal.');
});
const SpeechRecognition=window.SpeechRecognition||window.webkitSpeechRecognition;
document.getElementById('voiceBtn').addEventListener('click',()=>{
 if(!SpeechRecognition){say('O reconhecimento de voz não está disponível neste navegador. Use o Microsoft Edge ou Chrome.');return;}
 const rec=new SpeechRecognition();rec.lang='pt-BR';rec.interimResults=false;rec.maxAlternatives=1;
 say('Pode falar. Estou ouvindo.');
 rec.onresult=e=>{
  const command=e.results[0][0].transcript.toLowerCase();
  if(command.includes('youtube'))openApp('youtube');
  else if(command.includes('spotify'))openApp('spotify');
  else if(command.includes('rádio')||command.includes('radio'))openApp('radio');
  else if(command.includes('mapa')||command.includes('maps'))openApp('maps');
  else say('Entendi: '+command+'. Ainda estou aprendendo esse comando.');
 };
 rec.onerror=()=>say('Não consegui ouvir. Tente novamente.');
 rec.start();
});
function tick(){document.getElementById('clock').textContent=new Date().toLocaleTimeString('pt-BR',{hour:'2-digit',minute:'2-digit'});}
tick();setInterval(tick,10000);
