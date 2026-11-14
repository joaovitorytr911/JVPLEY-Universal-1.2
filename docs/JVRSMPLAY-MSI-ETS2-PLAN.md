# JVRSMPLAY — plano de integração MSI + ETS2

## Estado confirmado na branch `carpley-1.3`

O repositório-base é `joaovitorytr911/JVPLEY-Universal-1.2`. A branch de trabalho existente é `carpley-1.3` (também existe `android-carpley-1.3`).

Arquivos verificados:
- `app/index.html`: interface web da central multimídia.
- `app/style.css`: tema azul escuro/ciano.
- `app/app.js`: atalhos, links externos, relógio e assistente de voz do navegador.
- `launcher.py`: aguarda `eurotrucks2.exe` e abre a interface em janela de navegador.
- `JVPLEY-Iniciar.bat`: inicia o launcher Python.
- `README.md`: descreve o protótipo e suas limitações.

## Limitações atuais (não tratar como recursos prontos)

1. O protótipo abre uma janela externa; não está incorporado à tela física/virtual do painel do caminhão.
2. O painel de velocidade/combustível é demonstrativo; não existe telemetria real conectada.
3. Não foi encontrado código-fonte de plugin ETS2 nem uma DLL compilada nesta branch.
4. O iniciador atual depende de Python 3 e do navegador instalado.
5. Spotify, YouTube e Maps abrem páginas externas; a rádio exige uma fonte de streaming configurada.

## Arquitetura planejada

### 1. Aplicativo Windows
- Substituir o iniciador Python por um aplicativo Windows empacotado, sem exigir que o usuário instale Python ou abra um terminal.
- Manter a interface HTML/CSS/JS e hospedar a tela numa janela de aplicativo usando WebView2 (com verificação/instalação do runtime).
- Preservar o tema azul neon escolhido para o JVRSMPLAY.
- Abrir a central quando o ETS2 for detectado, sem forçar o jogo para fora da tela cheia e sem criar uma tela preta.

### 2. Ponte de telemetria ETS2
- Criar um componente x64 separado, baseado na documentação/SDK oficial de telemetria da SCS Software.
- Ler apenas os dados disponibilizados pelo jogo: velocidade, RPM, marcha, combustível, temperatura, posição e estado do motor, conforme suporte do SDK.
- Expor os dados à aplicação por uma interface local documentada (IPC), com tratamento de jogo fechado, dados indisponíveis e reconexão.
- Não mostrar valores simulados como se fossem telemetria real.
- Validar a pasta de plugins correta para a versão instalada do ETS2 antes de copiar qualquer DLL. Não presumir que a pasta Documentos\Euro Truck Simulator 2\plugins seja o destino de um plugin nativo.

### 3. Instalador MSI
- Usar WiX Toolset para produzir um pacote MSI x64.
- Instalar o aplicativo, os recursos da interface, a ponte de telemetria já compilada e os pré-requisitos necessários.
- Detectar a instalação do ETS2; se não for encontrada, instalar a central sem o componente de integração do jogo e informar isso claramente.
- Fazer backup de arquivos que seriam substituídos, nunca sobrescrever DLLs de terceiros silenciosamente e oferecer remoção limpa.
- Criar atalho no Menu Iniciar e, opcionalmente, na área de trabalho.
- Validar instalação, atualização e desinstalação em Windows 11 antes de distribuir o MSI.

## Critérios para declarar a versão pronta

- MSI x64 gerado e testado.
- Aplicativo inicia sem exigir Python/VS Code.
- ETS2 1.61 detectado corretamente.
- Telemetria real confirmada em jogo, com dados indisponíveis tratados corretamente.
- Nenhuma tela preta ou alteração indevida do modo de exibição do ETS2.
- Instalação e desinstalação verificadas.
- DLL e executáveis identificados, com origem e versão; não distribuir binários não verificados.

## Próxima etapa técnica

Implementar primeiro o host Windows sem Python e a interface IPC da telemetria. Só depois incluir a DLL real e os seus binários no pacote WiX/MSI. Este documento é um plano técnico; não representa uma DLL ou MSI já compilado.
