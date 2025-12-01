namespace Naviguard.Domain.Entities
{
    public class BusinessDepartment
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        public override string ToString() => DepartmentName;
    }
}