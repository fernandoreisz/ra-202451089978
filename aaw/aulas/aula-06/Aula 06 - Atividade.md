# Atividade — AULA 06

## Síncrono ou Assíncrono?

*Análise de fluxos de comunicação entre serviços — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

Vocês são os arquitetos dos 4 fluxos abaixo. Para CADA cenário:

- Decidam o estilo de comunicação: síncrono (request/response), assíncrono (fila/evento) ou API Gateway/BFF
- Desenhem o fluxo com caixas (serviços) e setas (chamadas/mensagens) no espaço indicado
- Justifiquem com pelo menos 2 fatores (urgência da resposta, tolerância a atraso, picos, falhas...)
- Apontem o principal risco da escolha de vocês

*⏱️ Tempo: 25 minutos  |  👥 Formato: em duplas  |  Não existe resposta única — o que vale é a justificativa.*

> **Nomes:** Fernando Reis  **Turma:** AAW - ADS   **Data:** 10 / 09 / 2026

## CENÁRIO 01 — PagFácil — aprovar ou negar AGORA

No checkout do PagFácil, ao clicar em “Pagar”, o serviço de Pagamentos precisa consultar o saldo/limite do cliente no serviço de Contas — e a resposta define se a venda acontece neste exato momento.

- O cliente está na tela, esperando o resultado da compra
- Sem a resposta de Contas, não há decisão possível: aprovar às cegas é proibido
- Tempo de resposta do serviço de Contas: ~80 ms em condições normais

**Sua análise:**

1. Estilo recomendado:   x Síncrono      ☐ Assíncrono (fila/evento)      ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

|Cliente  | pagamentos ---- requisição ao banco ------> saldo conta
                        |-
                        |-
                         -
                         aprova ou nega a transição

3. Justificativa (mínimo 2 fatores):
O cliente está esperando a resposta para saber se a compra foi aprovada ou negada. Além disso, não é permitido aprovar a compra sem consultar o saldo. Como o serviço de Contas tem a resposta rápida, a comunicação síncrona é adequada.

4. Principal risco da escolha:
Se o serviço de contas ficar fora do ar ou demorar muito para responder, a compra pode ficar travada ou falhar.

## CENÁRIO 02 — CadastraJá — o e-mail de boas-vindas

Após criar a conta no CadastraJá, o sistema envia um e-mail de boas-vindas. O provedor de e-mail às vezes demora 8 segundos para responder e falha em 2% das tentativas.

- O usuário quer começar a usar o app imediatamente após o cadastro
- O e-mail chegar 1 minuto depois não incomoda ninguém
- Se o provedor falhar, o envio deve ser tentado de novo — sem o usuário perceber

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono     x Assíncrono (fila/evento)      ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

| Cliente | Cadastro | Fila | compra ----> 
| serviço com o email |

3. Justificativa (mínimo 2 fatores):
Com uma fila, o cadastro pode ser concluído rapidamente e o envio do e-mail pode ser solicitado depois, enquanto estiver usando o aplicativo 
4. Principal risco da escolha:
O principal risco é a mensagem da fila ser perdida ou processada de forma incorreta, fazendo o usuário não receber o e-mail.
## CENÁRIO 03 — MegaMarket — baixa de estoque nos picos

No marketplace MegaMarket, cada venda gera uma baixa no serviço de Estoque. Nas grandes promoções o tráfego sobe 10x e o Estoque não dá conta de responder na velocidade das vendas.

- Atraso de alguns segundos na baixa é aceitável
- PERDER uma baixa de estoque não é aceitável (gera venda sem produto)
- O checkout não pode ficar lento nem cair porque o Estoque está sobrecarregado

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      x Assíncrono (fila/evento)      ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):
venda
|marketplace  | fila
| serviço de estoque| ------> saldo do estoque

3. Justificativa (mínimo 2 fatores):
A baixa pode acontecer alguns segundos depois, então não é necessário bloquear o checkout esperando o Estoque. Além disso, nas promoções o tráfego aumenta 10 vezes, então a fila ajuda a absorver os picos e permite que o Estoque processe as baixas conforme sua capacidade.

4. Principal risco da escolha:
O principal risco é perder uma mensagem da fila e a baixa não ser realizada, causando uma venda de um produto que não existe.

## CENÁRIO 04 — AppBanco — uma tela, cinco serviços

A tela inicial do AppBanco mostra saldo, fatura do cartão, investimentos, empréstimos e cashback — dados de 5 serviços diferentes. O time mobile reclama: são 5 chamadas, 5 formatos de resposta e 5 pontos de falha em cada abertura do app.

- A tela precisa abrir rápido, inclusive em redes móveis ruins
- Cada serviço tem equipe, formato e autenticação próprios
- Amanhã nasce a versão web, que precisa de MAIS dados que a mobile

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      ☐ Assíncrono (fila/evento)      x  API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

| app mobile |
| api gateway|
---------------- > fatura ---> investimento --->empréstimos--->cashback
3. Justificativa (mínimo 2 fatores):
O aplicativo faz uma chamada para os 5 elementos, também pode adaptar os dados para cada cliente, como mobile e web, que podem precisar de informações diferentes.
4. Principal risco da escolha:

## DESAFIO

1. Escolha um cenário em que vocês indicaram ASSÍNCRONO. Os brokers de mensagens costumam garantir entrega “pelo menos uma vez” — ou seja, a MESMA mensagem pode chegar duas vezes. O que aconteceria no seu fluxo? Como o consumidor deveria se proteger?
 MegaMarket - 03
 No cenário do estoque, a mesma mensagem pode chegar duas vezes e causar duas baixas. O consumidor deve verificar se o evento já foi processado e ignorar a segunda mensagem. Assim, evitamos duplicidade na baixa do estoque.
 Para evitar esse problema, o consumidor deve ser idempotente, verificando se aquele evento já foi processado antes de realizar a baixa novamente. Assim, mesmo que a mesma mensagem chegue duas vezes, o estoque não será baixado duas vezes.