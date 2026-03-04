namespace Barbershop.Domain.Abstractions
{
    public abstract class AuditableEntity : Entity
    {
        protected AuditableEntity(Guid id) : base(id)
        {
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public DateTime CreatedAtUtc { get; protected set; }
        public DateTime? UpdatedAtUtc { get; protected set; }

        public void SetUpdated(DateTime updatedAtUtc)
        {
            UpdatedAtUtc = updatedAtUtc;
        }
    }
}
