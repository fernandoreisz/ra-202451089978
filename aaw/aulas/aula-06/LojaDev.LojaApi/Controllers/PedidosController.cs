using System.Text;
using System.Text.Json;
using LojaDev.Compartilhado.Fila;
using LojaDev.Compartilhado.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace LojaDev.LojaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly FilaDePedidos _fila;

    public PedidosController(
        IHttpClientFactory httpFactory,
        IConfiguration config,
        FilaDePedidos fila)
    {
        _httpFactory = httpFactory;
        _config = config;
        _fila = fila;
    }

    [HttpPost("sincrono")]
    public async Task<IActionResult> CriarPedidoSincrono([FromBody] Pedido pedido)
    {
        var inicioTotal = DateTime.Now;
        Console.WriteLine($"\n🛒 ═══ FLUXO SÍNCRONO — Pedido {pedido.Id} ═══");

        // TODO 1 — Chamar PagamentoApi via HTTP
        var urlPagamento = _config["ServicoPagamento"];
        var client = _httpFactory.CreateClient();
        var json = JsonSerializer.Serialize(pedido);
        var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

        var resposta = await client.PostAsync(
            $"{urlPagamento}/api/pagamentos",
            conteudo
        );

        if (!resposta.IsSuccessStatusCode)
        {
            return StatusCode((int)resposta.StatusCode, "Erro ao processar pagamento.");
        }

        var corpo = await resposta.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<JsonElement>(corpo);
        bool aprovado = resultado.GetProperty("aprovado").GetBoolean();

        if (!aprovado)
        {
            var tempoRejeicao = (DateTime.Now - inicioTotal).TotalMilliseconds;
            Console.WriteLine($"🛒 Pedido REJEITADO em {tempoRejeicao:F0}ms");
            return BadRequest(new
            {
                pedidoId = pedido.Id,
                status = "rejeitado",
                mensagem = "Pagamento não aprovado",
                tempoTotalMs = tempoRejeicao
            });
        }

        // TODO 2 — Chamar NotificacaoApi via HTTP
        var urlNotificacao = _config["ServicoNotificacao"];

        var bodyNotificacao = new
        {
            Destinatario = $"{pedido.Cliente.ToLower().Replace(" ", "")}@email.com",
            Assunto = "Pedido Confirmado",
            Corpo = $"Seu pedido do produto {pedido.Produto} foi aprovado com sucesso!"
        };

        var jsonNotif = JsonSerializer.Serialize(bodyNotificacao);
        var conteudoNotif = new StringContent(
            jsonNotif,
            Encoding.UTF8,
            "application/json"
        );

        var respostaNotif = await client.PostAsync(
            $"{urlNotificacao}/api/notificacoes",
            conteudoNotif
        );

        var tempoTotal = (DateTime.Now - inicioTotal).TotalMilliseconds;

        if (!respostaNotif.IsSuccessStatusCode)
        {
            return StatusCode(500, new
            {
                mensagem = "Pedido pago, mas falhou ao enviar notificação síncrona.",
                tempoTotalMs = tempoTotal
            });
        }

        Console.WriteLine($"🛒 ═══ SÍNCRONO concluído em {tempoTotal:F0}ms ═══\n");

        return Ok(new
        {
            pedidoId = pedido.Id,
            status = "aprovado",
            notificacao = "enviada",
            fluxo = "sincrono",
            tempoTotalMs = tempoTotal,
            observacao = "⏱️ Note o tempo total — inclui a espera da notificação!"
        });
    }

    [HttpPost("assincrono")]
    public async Task<IActionResult> CriarPedidoAssincrono([FromBody] Pedido pedido)
    {
        var inicioTotal = DateTime.Now;
        Console.WriteLine($"\n🛒 ═══ FLUXO ASSÍNCRONO — Pedido {pedido.Id} ═══");

        // TODO 3 — Chamar PagamentoApi via HTTP
        var urlPagamento = _config["ServicoPagamento"];
        var client = _httpFactory.CreateClient();
        var json = JsonSerializer.Serialize(pedido);
        var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

        var resposta = await client.PostAsync(
            $"{urlPagamento}/api/pagamentos",
            conteudo
        );

        if (!resposta.IsSuccessStatusCode)
        {
            return StatusCode((int)resposta.StatusCode, "Erro ao processar pagamento.");
        }

        var corpo = await resposta.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<JsonElement>(corpo);
        bool aprovado = resultado.GetProperty("aprovado").GetBoolean();

        if (!aprovado)
        {
            var tempoRejeicao = (DateTime.Now - inicioTotal).TotalMilliseconds;
            Console.WriteLine($"🛒 Pedido REJEITADO em {tempoRejeicao:F0}ms");
            return BadRequest(new
            {
                pedidoId = pedido.Id,
                status = "rejeitado",
                mensagem = "Pagamento não aprovado",
                tempoTotalMs = tempoRejeicao
            });
        }

        // TODO 4 — Publicar evento na fila
        var evento = new EventoPedidoAprovado
        {
            PedidoId = pedido.Id,
            Cliente = pedido.Cliente,
            Produto = pedido.Produto,
            Valor = pedido.Valor
        };

        await _fila.PublicarAsync(evento);

        // TODO 5 — Retornar HTTP 202
        var tempoTotal = (DateTime.Now - inicioTotal).TotalMilliseconds;
        Console.WriteLine($"🛒 ═══ ASSÍNCRONO concluído em {tempoTotal:F0}ms ═══");
        Console.WriteLine($"🛒 (notificação será processada em background)\n");

        return Accepted(new
        {
            pedidoId = pedido.Id,
            status = "aprovado",
            notificacao = "pendente — será processada em background",
            fluxo = "assincrono",
            tempoTotalMs = tempoTotal,
            observacao = "⚡ Compare este tempo com o fluxo síncrono!"
        });
    }
}
