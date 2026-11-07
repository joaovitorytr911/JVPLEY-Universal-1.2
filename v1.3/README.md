# JVPLEY Android CarPlay 1.3 — base de desenvolvimento

Esta pasta inicia a nova base sem sobrescrever o protótipo 1.2.

## Estado atual
- Interface web local: inicial.
- Ponte HTTP local: inicial.
- Telemetria real: ainda não conectada.
- App Android/iOS: ainda não implementado.
- Chamadas reais e Bluetooth: ainda não implementados; a interface não simula que estão conectados.

## Requisitos
- Windows 11
- Python 3
- Euro Truck Simulator 2 1.61 (para os testes de integração posteriores)

## Iniciar
Execute `INICIAR-JVPLEY-1.3.bat` nesta pasta. O servidor local abre em http://127.0.0.1:8765.

## Próximas etapas
1. Validar a ponte local.
2. Integrar um plugin de telemetria compatível com a versão do jogo e ler os dados documentados pelo SDK da SCS.
3. Testar dados em caminhões originais, um por vez.
4. Desenvolver o cliente Android e estudar as APIs suportadas para iOS.
5. Implementar mídia, assistente CARVOZ e chamadas respeitando as permissões do sistema operacional.

## Limites técnicos
A API de telemetria oficial fornece dados do veículo para aplicações externas. Ela não injeta automaticamente uma central na tela original de todos os caminhões. A integração visual no painel original exige trabalho específico nos arquivos de dashboard de cada caminhão. Compatibilidade universal só pode ser declarada após testes.

Referência: https://modding.scssoft.com/wiki/Documentation/Engine/SDK/Telemetry
