namespace Api.Models;

public class Activity : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public ICollection<League> Leagues { get; set; } = new List<League>();
}