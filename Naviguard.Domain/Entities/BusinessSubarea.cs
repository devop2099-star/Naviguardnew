namespace Naviguard.Domain.Entities
{
    public class BusinessSubarea
    {
        public int SubareaId { get; set; }
        public string SubareaName { get; set; } = string.Empty;
        public int AreaId { get; set; }

        public override string ToString() => SubareaName;
    }
}