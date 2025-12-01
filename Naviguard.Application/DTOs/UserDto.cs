namespace Naviguard.Application.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class UserCredentialDto
    {
        public long UserId { get; set; }
        public long PageId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class FilterUsersDto
    {
        public string? Name { get; set; }
        public int? DepartmentId { get; set; }
        public int? AreaId { get; set; }
        public int? SubareaId { get; set; }
    }
}