namespace Naviguard.Application.DTOs
{
    public class GroupDto
    {
        public long GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public short Pin { get; set; }
        public List<PageDto> Pages { get; set; } = new();
    }

    public class CreateGroupDto
    {
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPinned { get; set; }
        public List<long> PageIds { get; set; } = new();
    }

    public class UpdateGroupDto
    {
        public long GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public short Pin { get; set; }
        public List<PageAssignmentDto> Pages { get; set; } = new();
    }

    public class PageAssignmentDto
    {
        public long PageId { get; set; }
        public bool IsPinned { get; set; }
    }
}