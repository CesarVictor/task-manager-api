using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Model;
using TaskManagerAPI.Database;
using Swashbuckle.AspNetCore.Annotations;

namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly TaskContext _context;

        public CommentsController(TaskContext context)
        {
            _context = context;
        }

        // POST: api/Comments
        [HttpPost]
        [SwaggerOperation(
            Summary = "Créer un commentaire",
            Description = "Ajoute un nouveau commentaire à une tâche. Le commentaire doit inclure un TaskId valide."
        )]
        [SwaggerResponse(201, "Commentaire créé avec succès")]
        [SwaggerResponse(400, "Données invalides")]
        public async Task<ActionResult<Comment>> PostComment([FromBody] Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            var taskExists = await _context.Tasks.AnyAsync(t => t.Id == comment.TaskId);
            if (!taskExists)
            {
                return NotFound(new { error = "Task not found", taskId = comment.TaskId });
            }

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }

        // GET: api/Comments/Task/{taskId}
        [HttpGet("Task/{taskId}")]
        [SwaggerOperation(
            Summary = "Récupérer les commentaires d'une tâche",
            Description = "Retourne tous les commentaires associés à une tâche spécifique par son ID."
        )]
        [SwaggerResponse(200, "Liste des commentaires retournée avec succès")]
        [SwaggerResponse(404, "Aucun commentaire trouvé pour cette tâche")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByTask(int taskId)
        {
            var comments = await _context.Comments
                                         .Where(c => c.TaskId == taskId)
                                         .ToListAsync();

            if (!comments.Any())
            {
                return NotFound(new { error = "No comments found for the task", taskId });
            }

            return comments;
        }

        // GET: api/Comments/{id}
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Récupérer un commentaire par ID",
            Description = "Retourne les détails d'un commentaire spécifique en fonction de son ID."
        )]
        [SwaggerResponse(200, "Commentaire trouvé et retourné")]
        [SwaggerResponse(404, "Commentaire introuvable")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound(new { error = "Comment not found", commentId = id });
            }

            return comment;
        }

        // DELETE: api/Comments/{id}
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Supprimer un commentaire",
            Description = "Supprime un commentaire existant par son ID."
        )]
        [SwaggerResponse(204, "Commentaire supprimé avec succès")]
        [SwaggerResponse(404, "Commentaire introuvable")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound(new { error = "Comment not found", commentId = id });
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
