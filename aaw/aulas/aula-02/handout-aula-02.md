**# HANDOUT — AULA 02**

**## Dissecando o HTTP**

*6 requisições sob o microscópio — Arquitetura de Aplicações Web*

**## 🎯 MISSÃO**

Vocês interceptaram 6 conversas entre um app e a API de uma biblioteca. Para CADA card:

* Descrevam o que o cliente pediu (verbo + recurso na URI)

* Expliquem o que o status code da resposta informa

* Respondam: repetindo a MESMA requisição 3 vezes seguidas, o estado do servidor muda?

Ao final, preencham juntos a **TABELA-SÍNTESE** dos verbos na última página.

*⏱️ Tempo: 30 minutos | 👥 Formato: em duplas | Dica: o card 6 esconde uma pegadinha de quem é a culpa.*

> **Nomes:** Fernando josé dos reis cruz
> **Turma:** ADS
> **Data:** 20 / 08 / 2026

---

**## REQUISIÇÃO 01 — A prateleira inteira**

```text
→ REQUISIÇÃO

GET /api/livros HTTP/1.1

Host: biblioteca.newton.br

Accept: application/json
```

```text
← RESPOSTA

HTTP/1.1 200 OK

Content-Type: application/json

[ { "id": 1, "titulo": "Clean Code", "autor": "Robert C. Martin" },

  { "id": 7, "titulo": "O Programador Pragmático", "autor": "Hunt & Thomas" } ]
```

**Sua análise:**

1. **O que o cliente pediu (verbo + recurso)?**

O cliente lançou o comando para ler os recursos utilizando o GET.

2. **O que o status code informa? Deu certo? Culpa de quem se não deu?**

O status code 200 fala que deu tudo certo. E o 400 é erro por culpa do cliente.

3. **Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?**

Não. Como o GET apenas consulta os livros, ele não altera o estado do servidor.

---

**## REQUISIÇÃO 02 — O livro fantasma**

```text
→ REQUISIÇÃO

GET /api/livros/99 HTTP/1.1

Host: biblioteca.newton.br

Accept: application/json
```

```text
← RESPOSTA

HTTP/1.1 404 Not Found

Content-Type: application/problem+json

{ "title": "Not Found", "status": 404 }
```

**Sua análise:**

1. **O que o cliente pediu (verbo + recurso)?**

Ele quis consultar o livro 99 com o método GET.

2. **O que o status code informa? Deu certo? Culpa de quem se não deu?**

Erro do cliente pois o livro com id 99 não existe.

3. **Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?**

Não pois a requisição GET não muda o estado do servidor.

---

**## REQUISIÇÃO 03 — Livro novo na estante**

```text
→ REQUISIÇÃO

POST /api/livros HTTP/1.1

Host: biblioteca.newton.br

Content-Type: application/json

{ "titulo": "Domain-Driven Design", "autor": "Eric Evans" }
```

```text
← RESPOSTA

HTTP/1.1 201 Created

Location: /api/livros/8

Content-Type: application/json

{ "id": 8, "titulo": "Domain-Driven Design", "autor": "Eric Evans" }
```

**Sua análise:**

1. **O que o cliente pediu (verbo + recurso)?**

O cliente quis postar um livro novo com o método POST.

2. **O que o status code informa? Deu certo? Culpa de quem se não deu?**

O status code 201 informa que o livro foi criado.

4xx culpa do cliente se tivesse dado erro ou o 5xx do servidor.

3. **Enviando este POST 3 vezes seguidas, o que acontece na estante? Para que serve o header Location?**

Será criado 3 livros com IDs diferentes e o header Location informa a URL do recurso que acabou de ser criado.

---

**## REQUISIÇÃO 04 — Corrigindo a ficha completa**

```text
→ REQUISIÇÃO

PUT /api/livros/7 HTTP/1.1

Host: biblioteca.newton.br

Content-Type: application/json

{ "id": 7, "titulo": "O Programador Pragmático", "autor": "D. Hunt; D. Thomas" }
```

```text
← RESPOSTA

HTTP/1.1 200 OK

Content-Type: application/json

{ "id": 7, "titulo": "O Programador Pragmático", "autor": "D. Hunt; D. Thomas" }
```

**Sua análise:**

1. **O que o cliente pediu (verbo + recurso)?**

O cliente quis alterar os dados do livro 7 utilizando o método PUT.

2. **O que o status code informa? Deu certo? Culpa de quem se não deu?**

O status code 200 informa que a alteração foi realizada com sucesso.

Se desse erro, um 4xx seria relacionado ao cliente e um 5xx seria relacionado ao servidor.

3. **Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?**

Não. Como o PUT está colocando os mesmos dados no livro 7, repetir a requisição deixa o servidor no mesmo estado. A resposta continuará sendo 200.

---

**## REQUISIÇÃO 05 — Fora do catálogo**

```text
→ REQUISIÇÃO

DELETE /api/livros/7 HTTP/1.1

Host: biblioteca.newton.br
```

```text
← RESPOSTA

HTTP/1.1 204 No Content
```

**Sua análise:**

1. **O que o cliente pediu (verbo + recurso)?**

O cliente quis excluir o livro 7 utilizando o método DELETE.

2. **O que o status code informa? Deu certo? Culpa de quem se não deu?**

O status code 204 informa que o livro foi excluído com sucesso e que não existe conteúdo para retornar na resposta.

3. **Repetindo o DELETE, o estado do servidor muda? Que resposta você ESPERA na segunda vez?**

Na primeira vez o livro é excluído. Na segunda vez o livro já não existe, então o estado do servidor não muda. Eu esperaria um status 404 Not Found.

---

**## REQUISIÇÃO 06 — O cadastro capenga**

```text
→ REQUISIÇÃO

POST /api/livros HTTP/1.1

Host: biblioteca.newton.br

Content-Type: application/json

{ "autor": "Anônimo" }
```

```text
← RESPOSTA

HTTP/1.1 400 Bad Request

Content-Type: application/problem+json

{ "title": "Bad Request", "status": 400,

  "errors": { "Titulo": [ "O campo Titulo é obrigatório" ] } }
```

**Sua análise:**

1. **O que o cliente pediu (verbo + recurso)?**

O cliente quis cadastrar um novo livro utilizando o método POST, mas não informou o título.

2. **O que o status code informa? Deu certo? Culpa de quem se não deu?**

O status code 400 informa que a requisição está errada porque o campo título é obrigatório. A culpa é do cliente, pois ele não enviou todas as informações necessárias.

3. **Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?**

Não. O livro não será criado porque o título está faltando. Repetindo a requisição, o servidor continuará retornando 400 Bad Request e seu estado não muda.

---

**## TABELA-SÍNTESE — Os verbos do HTTP**

*“Seguro” = não altera nada no servidor. “Idempotente” = repetir N vezes deixa o servidor no mesmo estado que 1 vez.*

| **Verbo**    | **Para que serve**                       | **Seguro?** | **Idempotente?**    | **Status típicos** |
| ------------ | ---------------------------------------- | ----------- | ------------------- | ------------------ |
| **`GET`**    | Consultar ou buscar recursos             | Sim         | Sim                 | 200, 404           |
| **`POST`**   | Criar um novo recurso                    | Não         | Não                 | 201, 400           |
| **`PUT`**    | Alterar ou substituir um recurso inteiro | Não         | Sim                 | 200, 404           |
| **`PATCH`**  | Alterar apenas parte de um recurso       | Não         | Não necessariamente | 200, 404           |
| **`DELETE`** | Excluir um recurso                       | Não         | Sim                 | 204, 404           |

---

**## DESAFIO**

1. **O verbo PATCH não apareceu em nenhum card. Qual a diferença entre PATCH e PUT? Um app de banco quer alterar SÓ o apelido do usuário, entre dezenas de campos do perfil — qual dos dois você usaria e por quê?**

A diferença é que o PUT normalmente altera o recurso inteiro, enquanto o PATCH altera apenas uma parte do recurso.

Nesse caso eu usaria o PATCH, porque o aplicativo precisa alterar somente o apelido do usuário sem precisar enviar ou alterar todos os outros campos do perfil.
