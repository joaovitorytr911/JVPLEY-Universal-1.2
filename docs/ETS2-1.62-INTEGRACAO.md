# Integração JVRSMPLAY com ETS2 1.62

## Configuração-alvo confirmada
- Euro Truck Simulator 2 (ETS2) 1.62.x, Windows 11 x64.
- Mapa BR Brasil 6.2.
- Projeto: branch `carpley-1.3`.
- CAR VOZ removido.
- Voz de navegação GPS: feminina, português do Brasil.

## Objetivo da primeira integração
Receber telemetria verdadeira do jogo e exibi-la no JVRSMPLAY:
- velocidade;
- rotação do motor (RPM);
- marcha;
- combustível;
- posição do caminhão, quando disponibilizada pela API.

Nenhum valor demonstrativo pode ser apresentado como dado real do jogo.

## Arquitetura prevista
1. Um plugin nativo x64 do ETS2 usa a API/SDK de telemetria da SCS.
2. O plugin envia os dados para um canal local entre processos (IPC), restrito ao próprio computador.
3. O aplicativo Windows recebe e valida as mensagens antes de atualizar a interface.
4. Se o jogo fechar ou a conexão cair, a interface mostra claramente que a telemetria está desconectada.

## Trava de compatibilidade
A compatibilidade do SDK/plugin com ETS2 1.62 ainda precisa ser comprovada. Algumas implementações públicas consultadas documentam suporte apenas a versões antigas. Portanto:
- não copiar DLL de terceiros sem verificar origem, versão e compatibilidade;
- não distribuir uma DLL vazia ou apenas ilustrativa;
- compilar para Windows x64 e testar carregamento no ETS2 1.62 antes de chamar o plugin de pronto;
- instalar no diretório correto do jogo, sem sobrescrever plugins existentes e com opção de backup.

## GPS — etapa posterior
O GPS será desenvolvido depois da telemetria básica. A interface desejada é inspirada no Google Maps: rota azul, marcador azul do caminhão, distância até a próxima conversão em metros e orientação por voz feminina em português do Brasil. A compatibilidade do Mapa BR Brasil 6.2 e a disponibilidade dos dados de rota precisam ser verificadas; não se presume que o mod forneça uma rota pronta à aplicação.

## Critérios para considerar a integração pronta
- plugin x64 compilado a partir do código-fonte identificado;
- ETS2 1.62 carrega o plugin sem erro;
- velocidade, RPM, marcha e combustível conferem com o painel do jogo;
- desconexão e encerramento do jogo são tratados corretamente;
- pacote de instalação não substitui arquivos de terceiros.

Até cumprir esses critérios, a integração permanece em desenvolvimento e não deve ser anunciada como funcional.
