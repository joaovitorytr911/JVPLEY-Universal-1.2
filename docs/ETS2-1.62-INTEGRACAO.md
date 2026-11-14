# Integração JVRSMPLAY com ETS2 1.62

## Configuração-alvo
- Euro Truck Simulator 2 (ETS2) 1.62.x, Windows 11 x64.
- Mapa BR Brasil 6.2.
- Projeto: branch `carpley-1.3`.
- CAR VOZ removido.
- Voz de navegação GPS: feminina, português do Brasil.

## Primeira integração: consumo
O painel da multimídia deve mostrar somente o consumo médio/instantâneo de combustível em L/100 km. Não exibir velocidade, RPM, marcha ou nível de combustível como indicadores da telemetria.

O consumo precisa ser calculado com dados reais fornecidos pelo ETS2; não usar números fictícios. Se o jogo não fornecer diretamente o valor desejado, será necessário calcular a partir dos dados de combustível e distância, documentando a fórmula e tratando pausas/reabastecimentos corretamente.

## Arquitetura prevista
1. Plugin nativo x64 do ETS2 usa a API/SDK de telemetria da SCS.
2. Plugin envia os dados necessários ao cálculo por canal local entre processos, restrito ao próprio computador.
3. Aplicativo Windows valida os dados e atualiza somente o indicador de consumo.
4. Ao fechar o jogo ou perder a conexão, a interface informa que a telemetria está desconectada.

## Compatibilidade e segurança
A compatibilidade do SDK/plugin com ETS2 1.62 precisa ser comprovada antes da distribuição. Não copiar DLL de terceiros sem verificar origem, versão e compatibilidade; não distribuir DLL vazia ou ilustrativa. O plugin deve ser compilado para Windows x64 e testado no jogo. A instalação futura deve detectar o diretório correto do ETS2, preservar plugins existentes e oferecer backup.

## GPS — etapa posterior
GPS inspirado no Google Maps: rota azul, marcador azul do caminhão, distância até a próxima conversão em metros e orientação por voz feminina em português do Brasil. A disponibilidade dos dados de rota e a compatibilidade com o Mapa BR Brasil 6.2 precisam ser verificadas.

## Critérios para considerar a integração pronta
- plugin x64 compilado a partir de código-fonte identificado;
- ETS2 1.62 carrega o plugin sem erro;
- consumo exibido confere com os dados reais do jogo e com cálculo validado;
- desconexão e encerramento do jogo são tratados corretamente;
- instalador não substitui arquivos de terceiros.

Até cumprir esses critérios, a integração permanece em desenvolvimento e não deve ser anunciada como funcional.

## Auditoria técnica do plugin — 2026-11-14

### Estado encontrado no repositório
- Não há, nesta branch, código-fonte nativo do plugin ETS2 nem DLL x64 compilada.
- O workflow Windows atual publica o aplicativo e o MSI, mas não compila nem inclui um plugin ETS2.
- Portanto, o artefato atual não pode ser anunciado como integração com o jogo.

### Pesquisa de referência do SDK
- A API de telemetria da SCS usa um plugin nativo carregado pelo jogo e callbacks para registrar canais.
- O canal de consumo médio de combustível aparece em implementações públicas da API como TRUCK_CHANNEL_fuel_average_consumption.
- A documentação pública consultada para projetos de terceiros ainda lista suporte comprovado somente até ETS2 1.46 em algumas implementações. Isso não prova incompatibilidade com 1.62, mas também não prova compatibilidade.
- O plugin deste projeto deverá ser compilado a partir de código-fonte identificado e headers do SDK com licença e versão registradas. Não reutilizar DLL binária de terceiros.

### Próximas tarefas obrigatórias
1. Fixar a origem e a versão dos headers oficiais do SCS Telemetry SDK e registrar a licença.
2. Implementar uma DLL própria x64 que exporte as funções exigidas pela API de telemetria e registre apenas os canais necessários ao consumo.
3. Publicar o consumo por IPC local com formato e versão documentados, sem expor uma porta de rede.
4. Adicionar a compilação nativa x64 ao workflow Windows e verificar arquitetura, exports e presença da DLL no pacote.
5. Só depois realizar teste real no ETS2 1.62: carregamento do plugin, leitura de consumo e encerramento/desconexão.

**Status atual: integração ainda não implementada.** Esta auditoria não cria nem representa uma DLL funcional.
## Ponte local do aplicativo — 2026-11-14
- Implementado no aplicativo Windows um cliente Named Pipe chamado `JVRSMPLAY.Telemetry.v1`.
- Protocolo esperado: uma mensagem JSON por linha, no formato `{"type":"consumption","value":12.3,"unit":"L/100km"}`.
- O cliente aceita somente mensagens do tipo `consumption`, unidade exata `L/100km` e valor numérico finito não negativo; mensagens inválidas são ignoradas.
- A interface mostra o consumo recebido e o estado da conexão. Sem plugin conectado, continua mostrando `--` e não inventa valores.
- A conexão usa Named Pipe local, sem abrir porta TCP.

### Limite desta etapa
O lado cliente do canal local está implementado. O servidor do Named Pipe dentro da DLL nativa ainda não existe. Portanto, ainda não há plugin ETS2 compilado, nem leitura real do jogo. A compilação Windows precisa confirmar que o novo cliente C# compila antes de avançar para o servidor nativo.

## Referências técnicas consultadas
- O cabeçalho público espelhado `scssdk_telemetry.h` descreve a inicialização/exportação do plugin e as versões de API 1.00/1.01.
- Implementações públicas registram `TRUCK_CHANNEL_fuel_average_consumption` como canal de consumo médio. Essas referências servem para orientar a implementação, não comprovam suporte do ETS2 1.62.
- Não foi identificada nesta etapa uma confirmação oficial de compatibilidade da API com ETS2 1.62. Exigir teste real no jogo.

## Implementação nativa do plugin — 2026-11-14

A auditoria acima registra o estado anterior à implementação. Esta seção atualiza o estado atual da branch.

### SDK utilizado
- O workflow baixa o **SCS Telemetry SDK 1.16** do endereço oficial \`https://download.eurotrucksimulator2.com/scs_sdk_1_16.zip\`.
- O pacote do SDK é versão 1.16; a API de telemetria declarada nos headers é 1.01. São números diferentes e não devem ser confundidos.
- A página oficial da SCS lista o SDK 1.16 como versão estável mais recente consultada nesta revisão.
- A licença do SDK foi registrada em \`plugin/LICENSE-SCS-SDK.txt\`.

### Código criado
- \`plugin/TelemetryPlugin.cpp\`: DLL nativa que exporta \`scs_telemetry_init\` e \`scs_telemetry_shutdown\`.
- O plugin valida o jogo \`eut2\` e registra somente o canal \`truck.fuel.consumption.average\`, do tipo float.
- O SDK fornece esse canal em litros por quilômetro. O plugin converte o valor para L/100 km multiplicando por 100.
- O plugin publica mensagens JSON por Named Pipe local \`JVRSMPLAY.Telemetry.v1\`; não abre porta TCP.
- \`desktop/TelemetryPipeClient.cs\` recebe e valida o protocolo, e a interface limpa o indicador quando o SDK informa que o valor está indisponível.
- \`plugin/CMakeLists.txt\` exige compilação x64 com MSVC.

### Compilação e pacote
- O workflow baixa o SDK oficial, compila a DLL x64 e verifica a arquitetura PE AMD64 e as duas exportações obrigatórias.
- O pacote MSI inclui os arquivos do aplicativo e a pasta \`ETS2-Plugin\`; o artefato também contém um ZIP separado para o plugin.
- Nesta etapa a instalação do plugin no jogo ainda é manual: copiar a DLL para \`bin/win_x64/plugins\`, conforme \`plugin/INSTALL.txt\`. O MSI ainda não detecta nem altera automaticamente a pasta do ETS2.
- A compilação do workflow ainda precisa terminar com sucesso para confirmar que a DLL foi realmente gerada. A existência do código-fonte não equivale a uma DLL compilada.

### Limites que permanecem
- Ainda não há teste real de carregamento e leitura no ETS2 1.62. A versão estável do SDK e a API declarada não provam, por si só, compatibilidade com essa versão específica do jogo.
- Ainda não foi conferido no jogo se o canal entrega valores corretos em todos os caminhões originais e no mapa BR Brasil 6.2.
- Não anunciar a integração como funcional até a compilação do workflow e o teste real no ETS2 1.62.

### Protocolo IPC v1 atualizado
Cada linha JSON contém uma mensagem:
- Disponível: \`{"type":"consumption","available":true,"value":12.3,"unit":"L/100km"}\`
- Indisponível: \`{"type":"consumption","available":false}\`

O cliente só apresenta valores numéricos finitos, não negativos e com unidade exata \`L/100km\`.
