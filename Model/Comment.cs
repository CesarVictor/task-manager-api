namespace TaskManagerAPI.Model;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int TaskId { get; set; } // Relier le commentaire à une tâche
    public int UserId { get; set; } // Relier le commentaire à un utilisateur
    public DateTime CreatedAt { get; set; } // Date de création du commentaire
    public User? User { get; set; } // Navigation property vers l'utilisateur
}