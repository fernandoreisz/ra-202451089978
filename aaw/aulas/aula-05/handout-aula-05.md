# HANDOUT — AULA 05

## Escolha o Banco

*Persistência em arquiteturas distribuídas — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

Vocês são o time de arquitetura de dados contratado pelas 4 empresas abaixo. Para CADA cenário:

- Escolham o modelo de banco: relacional, documento, chave-valor ou grafo
- Justifiquem com pelo menos 2 fatores do contexto (estrutura dos dados, padrão de acesso, escala, consistência...)
- Apontem o principal risco da escolha de vocês

*⏱️ Tempo: 25 minutos  |  👥 Formato: em duplas  |  Não existe resposta única — o que vale é a justificativa.*

> **Nomes:** Paulo Junior e Fernando Reis   **Turma:** Arquitetura de Aplicações WEB   **Data:** 03 / 09 / 2026

## CENÁRIO 01 — TechStore — o catálogo camaleão

E-commerce com 80 mil produtos. Cada categoria tem atributos completamente diferentes: livro tem autor e número de páginas; notebook tem RAM e CPU; camiseta tem tamanho e cor.

- A cada categoria nova, o time faz ALTER TABLE e a tabela produtos já tem 92 colunas (a maioria NULL)
- O produto é quase sempre lido INTEIRO, de uma vez, para montar a página
- Novos atributos surgem toda semana — o marketing não espera o DBA
- Relatórios cruzando categorias são raros

**Sua análise:**

1. Modelo recomendado:   ☐ Relacional     x Documento     ☐ Chave-valor     ☐ Grafo

2. Justificativa (mínimo 2 fatores do contexto):
o documento pode ser melhor aproveitado se utilazarmos um sistema igual ao Mongo, e ao armazenar os produtos com suas categorias, a pesquisa das tabelas terão um melhor resultado porque apresentará as informações sem ter que criar outras tabelas. Todas as informações necessárias podem ser apresentadas em único documento.
3. Principal risco da escolha:
Falta de padronização dos atributos.
## CENÁRIO 02 — MegaCart — o carrinho da Black Friday

Serviço de carrinho de compras de um varejista gigante. Na Black Friday são milhões de leituras e escritas por minuto.

- O acesso é SEMPRE pela chave: “carrinho do cliente 12345” — nunca por busca ou filtro
- Todo carrinho expira automaticamente em 48h (TTL)
- Latência precisa ser de poucos milissegundos
- Perder um carrinho é chato, mas NÃO é tragédia — o cliente remonta

**Sua análise:**

1. Modelo recomendado:   ☐ Relacional     ☐ Documento     x Chave-valor     ☐ Grafo

2. Justificativa (mínimo 2 fatores do contexto):

o modelo chave-valor é o mais indicado porque o acesso ao carrinho acontece sempre por uma chave específica, como por exemplo “carrinho do cliente 12345”. Dessa forma, o sistema não precisa fazer buscas complexas ou filtros, apenas localizar rapidamente o valor associado àquela chave.

Além disso, esse modelo oferece baixa latência, respondendo em poucos milissegundos, o que é muito importante em períodos de alto volume como a Black Friday. Outro ponto importante é que o carrinho pode expirar automaticamente em 48 horas usando TTL, recurso muito comum em bancos chave-valor como Redis.

3. Principal risco da escolha:

O principal risco é a possibilidade de perda dos dados do carrinho caso ocorra alguma falha, principalmente se os dados estiverem armazenados em memória. Porém, nesse cenário, perder um carrinho não é considerado uma tragédia, pois o cliente pode montá-lo novamente.

## CENÁRIO 03 — PayBank — dinheiro não pode evaporar

Módulo de transferências de um banco. Uma transferência debita uma conta e credita outra — as duas operações têm que acontecer JUNTAS ou nenhuma acontece.

- Consistência forte exigida por lei — saldo errado é multa do Banco Central
- Auditoria cruza contas, clientes, agências e transações em relatórios complexos (joins)
- O esquema dos dados é estável há 10 anos
- Volume alto, mas previsível

**Sua análise:**

1. Modelo recomendado:   x Relacional     ☐ Documento     ☐ Chave-valor     ☐ Grafo

2. Justificativa (mínimo 2 fatores do contexto):

o modelo relacional é o mais indicado porque trabalha muito bem com transações, garantindo que as operações aconteçam com segurança. No caso do banco, o débito de uma conta e o crédito em outra precisam acontecer juntos, ou nenhuma das duas operações pode ser concluída.

Além disso, o banco precisa de consistência forte e auditoria com relatórios complexos, cruzando contas, clientes, agências e transações. Como o esquema dos dados é estável e não muda com frequência, o banco relacional atende melhor esse cenário.

3. Principal risco da escolha:

Maior dificuldade para escalar horizontalmente e menor flexibilidade para mudanças rápidas no modelo de dados.

## CENÁRIO 04 — FriendLink — amigos dos seus amigos

Rede social profissional em que o produto principal é a indicação: “pessoas que você talvez conheça” e “quem pode te apresentar à empresa X”.

- As consultas dominantes percorrem RELACIONAMENTOS: amigos dos amigos, caminhos de indicação com até 6 níveis
- Em banco relacional, cada nível vira um self-join — com 6 níveis a consulta já não responde
- Os dados de perfil são simples; o valor está nas CONEXÕES
- O grafo cresce milhões de arestas por dia

**Sua análise:**

1. Modelo recomendado:   ☐ Relacional     ☐ Documento     ☐ Chave-valor     x Grafo

2. Justificativa (mínimo 2 fatores do contexto):

o modelo de grafo é o mais indicado porque o principal valor do sistema está nos relacionamentos entre as pessoas. Como a rede social precisa encontrar amigos dos amigos e caminhos de indicação com vários níveis, o banco de grafo consegue percorrer essas conexões de forma mais natural.

No banco relacional, seria necessário fazer vários self-joins para encontrar essas relações, o que deixaria a consulta pesada e lenta. Como os dados de perfil são simples e o mais importante são as conexões, o modelo de grafo atende melhor esse cenário.

3. Principal risco da escolha:

O principal risco é a complexidade de manter e escalar o grafo quando ele cresce muito, principalmente com milhões de novas conexões por dia. Também pode ser mais difícil fazer relatórios tradicionais quando comparado ao banco relacional.

## DESAFIO

1. Escolha um dos cenários e responda: se a rede particionar (metade dos servidores não enxerga a outra metade), o que o sistema deve fazer — parar de responder para não errar, ou continuar respondendo mesmo arriscando dados desatualizados? Qual letra do CAP vocês sacrificariam e por quê?

Cenário escolhido: CENÁRIO 03 — PayBank — dinheiro não pode evaporar

No cenário PayBank, em caso de partição da rede, o sistema deve parar de responder para não correr o risco de processar dados incorretos. Como se trata de dinheiro e transferências bancárias, a consistência dos dados é mais importante do que manter o sistema disponível a qualquer custo. Por isso, a letra sacrificada do CAP seria a **Disponibilidade (A)**, mantendo a Consistência (C) e a Tolerância à Partição (P).
