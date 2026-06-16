using System.ComponentModel.DataAnnotations;

namespace GLMS.Api.Models;

public class User
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(40)]
    public string Role { get; set; } = "Admin";
}
