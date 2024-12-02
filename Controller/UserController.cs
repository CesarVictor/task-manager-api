using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Model;
using TaskManagerAPI.Database;
using Swashbuckle.AspNetCore.Annotations;

namespace TaskManagerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly TaskContext _context;

    public UsersController(TaskContext context)
    {
        _context = context;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Récupérer tous les utilisateurs",
        Description = "Retourne la liste de tous les utilisateurs enregistrés, ainsi que les tâches qui leur sont assignées."
    )]
    [SwaggerResponse(200, "Liste d'utilisateurs retournée avec succès")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.Include(u => u.Tasks).ToListAsync();
    }

    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Récupérer un utilisateur par ID",
        Description = "Retourne les détails d'un utilisateur spécifique ainsi que les tâches associées."
    )]
    [SwaggerResponse(200, "Utilisateur trouvé et retourné")]
    [SwaggerResponse(404, "Utilisateur introuvable")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.Include(u => u.Tasks).FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { error = "User not found", userId = id });
        }

        return user;
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Créer un nouvel utilisateur",
        Description = "Ajoute un utilisateur à la base de données. Les informations de l'utilisateur doivent être fournies dans le corps de la requête."
    )]
    [SwaggerResponse(201, "Utilisateur créé avec succès")]
    [SwaggerResponse(400, "Données invalides")]
    public async Task<ActionResult<User>> CreateUser([FromBody] User user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Mettre à jour un utilisateur existant",
        Description = "Met à jour les informations d'un utilisateur. Les données mises à jour doivent être fournies dans le corps de la requête."
    )]
    [SwaggerResponse(204, "Utilisateur mis à jour avec succès")]
    [SwaggerResponse(400, "Données invalides ou ID incorrect")]
    [SwaggerResponse(404, "Utilisateur introuvable")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
    {
        if (id != user.Id)
        {
            return BadRequest(new { error = "User ID mismatch" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
            {
                return NotFound(new { error = "User not found", userId = id });
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Supprimer un utilisateur",
        Description = "Supprime un utilisateur existant de la base de données selon son ID."
    )]
    [SwaggerResponse(204, "Utilisateur supprimé avec succès")]
    [SwaggerResponse(404, "Utilisateur introuvable")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { error = "User not found", userId = id });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [NonAction]
    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}
