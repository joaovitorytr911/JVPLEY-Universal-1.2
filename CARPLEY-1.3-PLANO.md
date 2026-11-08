# CARPLEY 1.3 — plano de evolução

Este trabalho parte do JVPLEY Universal 1.2. A versão 1.2 permanece preservada na branch `main`.

## Estado confirmado da versão 1.2

- Interface web externa em HTML, CSS e JavaScript.
- Iniciador Python que detecta o processo `eurotrucks2.exe`.
- O iniciador abre a interface no navegador; não integra a central original do caminhão.
- Telemetria ainda demonstrativa, sem leitura real dos dados do jogo.
- YouTube, Spotify e Maps são abertos como páginas externas.
- Rádio depende de uma fonte de streaming autorizada.

## Objetivo da versão 1.3

- Preservar a identidade visual CARPLEY 1.3 aprovada.
- Preparar uma execução em tela cheia, sem tela preta na abertura.
- Investigar uma integração real e compatível com ETS2 antes de prometer funcionamento dentro do painel.
- Só gerar instalador Windows quando a aplicação estiver compilável e testável.
- Só incluir DLL quando existir um plugin real, com interface e integração verificadas.

## Etapas

1. Adaptar a interface 1.2 para o layout CARPLEY 1.3.
2. Definir a arquitetura do aplicativo Windows e o modo de tela cheia.
3. Investigar a API/SDK de telemetria e as possibilidades reais de integração do ETS2.
4. Implementar e testar a comunicação com o jogo.
5. Gerar instalador e validar em uma instalação limpa do Windows 11.

## Regras de qualidade

- Não substituir nem apagar a versão 1.2.
- Não usar valores fictícios como se fossem telemetria real.
- Não anunciar MSI, EXE ou DLL como prontos antes de gerar e testar os arquivos.
