# JVPLEY ETS2 Telemetry Plugin

Projeto nativo do plugin de telemetria do JVPLEY para Euro Truck Simulator 2.

## Objetivo
Criar uma DLL Windows x64 que use o SCS Telemetry SDK para receber dados reais do caminhão e disponibilizá-los ao aplicativo JVPLEY.

## Escopo inicial
- Alvo: Euro Truck Simulator 2 1.61, Windows 11, x64.
- Primeiros dados: velocidade, RPM, marcha, combustível e estado do jogo.
- Depois: posição do caminhão, coordenadas, temperatura e demais sinais disponibilizados pelo SDK.

## Arquitetura prevista
1. O ETS2 carrega a DLL na pasta `bin/win_x64/plugins`.
2. A DLL registra os callbacks exigidos pelo SCS Telemetry SDK.
3. Os callbacks atualizam um estado interno de telemetria.
4. O plugin publica os dados para o aplicativo JVPLEY por um canal local entre processos.
5. O aplicativo mostra os valores na central multimídia.

## SDK
Usar a documentação oficial do SCS Telemetry SDK. A página oficial lista a versão 1.16 como estável:
https://modding.scssoft.com/wiki/Documentation/Engine/SDK/Telemetry

## Estado
Este diretório documenta a implementação inicial. Ainda não contém uma DLL compilada nem foi testado no ETS2 1.61. Não instalar arquivos incompletos como se fossem um plugin funcional.

## Próximas etapas
- Incluir os headers e exemplos oficiais do SDK.
- Implementar os pontos de entrada e callbacks exigidos pela versão do SDK.
- Implementar a comunicação local com o aplicativo.
- Compilar em Windows x64.
- Testar carregamento e telemetria no ETS2 1.61.
