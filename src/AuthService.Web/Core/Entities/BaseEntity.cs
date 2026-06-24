namespace AuthService.Web.Core.Entities;

public abstract class BaseEntity<TKey>
{
    public TKey Id { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
