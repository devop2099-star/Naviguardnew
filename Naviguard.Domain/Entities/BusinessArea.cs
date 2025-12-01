namespace Naviguard.Domain.Entities
{
    public class BusinessArea
    {
        public int AreaId { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }

        public override string ToString() => AreaName;
    }
}