# JVRSMPLAY — branch carpley-1.3

Projeto experimental de central multimídia para Windows 11 e Euro Truck Simulator 2.

## Estado atual
- Aplicativo Windows x64 com interface multimídia.
- Controle de volume da voz e dos sons internos do aplicativo.
- Cliente de telemetria local preparado para receber somente consumo em L/100 km.
- Código-fonte inicial de plugin nativo ETS2 x64 em plugin/.
- Workflow Windows configurado para baixar o SCS Telemetry SDK 1.16, compilar a DLL, validar arquitetura/exportações e empacotar o instalador e o plugin.

## Como a telemetria está planejada
O plugin registra o canal de consumo médio do SDK da SCS e transmite o valor ao aplicativo por Named Pipe local. O painel não inventa valores: sem dados válidos, mostra --.

## Limitações atuais
- A DLL ainda precisa ser compilada com sucesso pelo GitHub Actions.
- A compatibilidade com ETS2 1.62 ainda precisa ser confirmada em teste real no jogo.
- O plugin ainda deve ser copiado manualmente para bin/win_x64/plugins; a instalação automática no diretório do ETS2 não foi implementada.
- O painel do aplicativo ainda é uma janela externa/overlay de teste, não a tela original da cabine do caminhão.
- YouTube, Spotify e Maps são abertos em páginas externas; a rádio depende de uma fonte de streaming autorizada.

## Documentação
- docs/ETS2-1.62-INTEGRACAO.md — arquitetura, protocolo IPC, limitações e critérios de teste.
- plugin/INSTALL.txt — instruções de instalação manual para teste.
- plugin/LICENSE-SCS-SDK.txt — aviso de licença do SDK da SCS.

Não distribua nem instale a DLL antes de confirmar a execução bem-sucedida do workflow e fazer backup da pasta de plugins do jogo.
