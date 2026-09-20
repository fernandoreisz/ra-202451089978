using ApiVazada.Data;
using ApiVazada.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiVazada.Controllers;

public record AtualizarUsuarioRequest(string Nome, string Email);

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsuariosController(AppDbContext db) => _db = db;

    private int UsuarioLogadoId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var usuario = _db.Usuarios.Find(id);
        if (usuario is null) return NotFound();

        return Ok(new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Role
        });
    }

    // ===============================
    // FALHA 4 (não corrija ainda!)
    // Mass Assignment:
    // o cliente consegue enviar campos que
    // não deveria poder alterar, como Role.
    // ===============================

    [Authorize]
    [HttpPut("{id}")]
    public IActionResult Atualizar(
        int id,
        [FromBody] AtualizarUsuarioRequest dados)
    {
        var usuario = _db.Usuarios.Find(id);

        if (usuario is null)
            return NotFound();

        if (usuario.Id != UsuarioLogadoId)
            return Forbid();

        usuario.Nome = dados.Nome;
        usuario.Email = dados.Email;

        _db.SaveChanges();

        return Ok(new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Role
        });
    }
}