using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Model;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<Tasks>? Tasks { get; set; }
}
