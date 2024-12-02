using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Model;

public class Tasks
{
    [Key] // Indique explicitement que "Id" est la clé primaire
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Le titre est obligatoire.")]
    [StringLength(100, ErrorMessage = "Le titre ne peut pas dépasser 100 caractères.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "La description est obligatoire.")]
    [StringLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Le statut est obligatoire.")]
    [RegularExpression("^(En attente|En cours|Terminée)$", ErrorMessage = "Le statut doit être 'En attente', 'En cours' ou 'Terminée'.")]
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; } 

    // Clé étrangère
    public int? UserId { get; set; }
    public User? AssignedUser { get; set; } // Navigation property vers l'utilisateurs

    public ICollection<Comment>? Comments { get; set; } // Navigation property vers les commentaires

}
