using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Model;
using TaskManagerAPI.Database;
using CsvHelper;
using System.ComponentModel.DataAnnotations;


using Swashbuckle.AspNetCore.Annotations;

namespace TaskManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly TaskContext _context;

        public TaskController(TaskContext context)
        {
            _context = context;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Récupérer toutes les tâches",
            Description = "Récupère toutes les tâches enregistrées dans la base de données. Permet de filtrer par statut."
        )]
        [SwaggerResponse(200, "Liste de toutes les tâches retournée avec succès")]
        [SwaggerResponse(400, "Requête invalide")]
        public async Task<IActionResult> GetTasks([FromQuery] string status = null)
        {
            var tasks = string.IsNullOrEmpty(status)
                ? await _context.Tasks.Include(t => t.AssignedUser).ToListAsync()
                : await _context.Tasks.Include(t => t.AssignedUser)
                    .Where(t => t.Status == status)
                    .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("{id}")]
         [SwaggerOperation(
            Summary = "Récupérer une tâche par ID",
            Description = "Récupère une tâche spécifique selon son identifiant unique."
        )]
        [SwaggerResponse(200, "Tâche trouvée et retournée")]
        [SwaggerResponse(404, "Tâche introuvable")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedUser)
                .Include(t => t.Comments) 

                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound(new { message = "Task not found" });

            return Ok(task);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Créer une nouvelle tâche",
            Description = "Ajoute une nouvelle tâche à la base de données. Les données de la tâche doivent être fournies dans le corps de la requête."
        )]
        [SwaggerResponse(201, "Tâche créée avec succès")]
        [SwaggerResponse(400, "Données invalides")]
        public async Task<IActionResult> CreateTask([FromBody] Tasks task)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            task.CreatedAt = DateTime.UtcNow;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }


        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Mettre à jour une tâche existante",
            Description = "Met à jour une tâche existante selon son ID. Les données mises à jour doivent être fournies dans le corps de la requête."
        )]
        [SwaggerResponse(204, "Tâche mise à jour avec succès")]
        [SwaggerResponse(404, "Tâche introuvable")]
        [SwaggerResponse(400, "Données invalides ou ID incorrect")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] Tasks updatedTask)
        {
            if (id != updatedTask.Id)
                return BadRequest(new { error = "Task ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound(new { error = "Task not found", taskId = id });

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.Status = updatedTask.Status;
            task.UserId = updatedTask.UserId;

            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Supprimer une tâche",
            Description = "Supprime une tâche existante selon son ID."
        )]
        [SwaggerResponse(204, "Tâche supprimée avec succès")]
        [SwaggerResponse(404, "Tâche introuvable")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound(new { message = "Task not found" });

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("assign/{taskId}/{userId}")]
        [SwaggerOperation(
        Summary = "Assigner une tâche à un utilisateur",
        Description = "Permet d'assigner une tâche existante à un utilisateur en fournissant leur ID respectif."
        )]
        [SwaggerResponse(200, "Tâche assignée avec succès")]
        [SwaggerResponse(404, "Tâche ou utilisateur introuvable")]
        public async Task<IActionResult> AssignTaskToUser(int taskId, int userId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return NotFound(new { error = "Task not found", taskId });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found", userId });

            task.UserId = userId;
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task assigned successfully", task });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetTaskStats()
        {
            var totalTasks = await _context.Tasks.CountAsync();
            var tasksByStatus = await _context.Tasks
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var tasksByUser = await _context.Users
                .Select(u => new
                {
                    UserName = u.Name,
                    TaskCount = u.Tasks.Count
                })
                .ToListAsync();

            var stats = new
            {
                TotalTasks = totalTasks,
                TasksByStatus = tasksByStatus,
                TasksByUser = tasksByUser
            };

            return Ok(stats);
        }
        
        [HttpGet("search")]
        [SwaggerOperation(
            Summary = "Rechercher des tâches",
            Description = "Rechercher des tâches dans la base de données en fonction de mots-clés, de l'utilisateur assigné ou de la date de création."
        )]
        [SwaggerResponse(200, "Tâches correspondant aux critères de recherche")]
        [SwaggerResponse(400, "Requête invalide")]
        public async Task<IActionResult> SearchTasks([FromQuery] string keyword, [FromQuery] int? assignedTo, [FromQuery] DateTime? createdAfter)
        {
        var query = _context.Tasks.AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(t => t.Title.Contains(keyword) || t.Description.Contains(keyword));

        if (createdAfter.HasValue)
            query = query.Where(t => t.CreatedAt >= createdAfter.Value);

        var results = await query.ToListAsync();
        return Ok(results);
        }

        [HttpGet("export")]
        [SwaggerOperation(
            Summary = "Exporter les tâches en CSV",
            Description = "Exporte toutes les tâches disponibles dans la base de données sous forme de fichier CSV."
        )]
        [SwaggerResponse(200, "Fichier CSV généré avec succès", contentTypes: "text/csv")]
        [SwaggerResponse(204, "Aucune tâche à exporter")]
        public async Task<IActionResult> ExportTasksToCsv()
        {
            var tasks = await _context.Tasks.ToListAsync();

            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(tasks);
            }

            return File(memoryStream.ToArray(), "text/csv", "tasks_export.csv");
        }


    }
}
