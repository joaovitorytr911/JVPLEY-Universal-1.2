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