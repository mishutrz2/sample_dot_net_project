namespace Api.Models;

using System.Text.Json.Serialization;

public class Activity : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    [JsonIgnore]
    public ICollection<League> Leagues { get; set; } = new List<League>();
}