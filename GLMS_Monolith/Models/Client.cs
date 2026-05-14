using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace GLMS_Monolith.Models;

public class Client
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string ContactDetails { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Region { get; set; } = string.Empty;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}