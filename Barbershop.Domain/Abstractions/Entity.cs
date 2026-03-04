namespace Barbershop.Domain.Abstractions
{
    public abstract class Entity(Guid id) : IEquatable<Entity>
    {
        public Guid Id { get; protected set; } = id;

        public bool Equals(Entity? other) => other is not null && Id == other.Id;

        public override bool Equals(object? obj) => obj is Entity entity && Equals(entity);

        public override int GetHashCode() => Id.GetHashCode();
    }
}
