# JVPLEY ETS2 Telemetry Plugin — desenvolvimento

Este diretório contém a primeira implementação nativa do plugin JVPLEY para Euro Truck Simulator 2 em Windows x64.

## O que o código faz nesta etapa

- Usa a API de telemetria do SCS (interface de compatibilidade 1.00).
- Aceita somente o identificador de jogo ETS2 (eut2); rejeita ATS.
- Registra os canais de velocidade, RPM, marcha e combustível.
- Acompanha os eventos de jogo pausado/em direção.
- Grava os dados recebidos em JSON, até aproximadamente 10 atualizações por segundo, em:

%LOCALAPPDATA%\JVPLEY\telemetry.json

Os dados indisponíveis são gravados como null, em vez de inventar valores.

## Compilação automatizada

O workflow .github/workflows/build-jvpley-msi.yml foi preparado para:

1. Baixar o pacote oficial SCS Telemetry SDK 1.16 durante a compilação.
2. Compilar JVPLEY.dll para Windows x64 usando MSVC.
3. Verificar a arquitetura x64 e os dois pontos de entrada exigidos pelo SCS.
4. Criar JVPLEY-Universal-1.3.msi com WiX.
5. Publicar os dois arquivos como artefato de desenvolvimento do GitHub Actions.

O MSI instala a DLL em Program Files\JVPLEY Universal 1.3 e registra o caminho completo na chave de plugins do Euro Truck Simulator 2 no Windows. Essa é uma forma de carregamento prevista pela documentação do SDK; por isso, não é necessário duplicar a mesma DLL na pasta bin\win_x64\plugins.

## Limitações atuais — não é lançamento final

- Ainda é necessário que o workflow termine com sucesso para existir uma DLL/MSI compilados.
- O plugin ainda não foi testado dentro do ETS2 1.61.
- O aplicativo/central multimídia atual ainda não lê o arquivo JSON; a ligação entre o plugin e a interface é uma etapa posterior.
- Esta etapa não implementa GPS, temperatura, pressão dos pneus, Bluetooth, Spotify, YouTube, Rádio FM ou CAR VOZ.
- O código não modifica a tela original da cabine do caminhão.

A versão do pacote SCS SDK (1.16) e a versão pública do jogo (1.61) são identificadores diferentes. A compatibilidade real com a instalação do usuário só poderá ser confirmada depois da compilação e do teste no jogo.

## Referências

Documentação oficial SCS Telemetry SDK: https://modding.scssoft.com/wiki/Documentation/Engine/SDK/Telemetry
Pacote oficial baixado no workflow: https://download.eurotrucksimulator2.com/scs_sdk_1_16.zip
