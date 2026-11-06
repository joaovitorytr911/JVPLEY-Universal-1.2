# JVPLEY Universal 1.2

Central multimídia experimental em português para Windows 11 e Euro Truck Simulator 2.

## Recursos do protótipo
- Interface futurista azul, com a assistente SCANIA.
- Atalhos: F1 menu, F2 YouTube, F3 Spotify, F4 Rádio, F5 Maps.
- Navegação com setas, Enter e Esc.
- Fala em português brasileiro usando as vozes disponíveis no navegador.
- Reconhecimento de comandos de voz, se o navegador oferecer suporte.
- Iniciador que detecta o processo eurotrucks2.exe e abre a interface.

## Instalação e uso
1. Baixe o projeto pelo botão **Code → Download ZIP**.
2. Extraia o arquivo ZIP no Windows.
3. Abra a pasta extraída e execute `JVPLEY-Iniciar.bat`.
4. O iniciador aguarda o ETS2 e abre a interface quando o jogo estiver rodando.
5. Para testar a interface sem abrir o jogo, execute `py launcher.py --demo` em um terminal.

Requer Python 3 no Windows para o iniciador. A interface é aberta no navegador padrão.

## Limitações importantes
Esta versão é um protótipo externo: não aparece dentro da tela original do painel do caminhão e ainda não lê telemetria real. Os números do painel são demonstrativos. A integração na cabine exige adaptar os arquivos de dashboard de cada caminhão/mod e implementar uma ligação com os dados do jogo.

YouTube, Spotify e Google Maps são abertos como páginas externas. A rádio precisa de um endereço de streaming oficial/autorizado configurado. O reconhecimento de voz depende do navegador e das permissões de microfone. A voz é a voz pt-BR disponível no sistema/navegador, não uma cópia da voz do filme.
