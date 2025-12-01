namespace Naviguard.Domain.Common
{
    public abstract class Entity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public short State { get; set; } = 1; // 1 = Activo, 0 = Inactivo

        protected Entity()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}