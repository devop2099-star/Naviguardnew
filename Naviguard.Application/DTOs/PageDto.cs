namespace Naviguard.Application.DTOs
{
    public class PageDto
    {
        public long PageId { get; set; }
        public string PageName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool RequiresProxy { get; set; }
        public bool RequiresLogin { get; set; }
        public bool RequiresCustomLogin { get; set; }
        public bool RequiresRedirects { get; set; }
        public short PinInGroup { get; set; }
    }

    public class CreatePageDto
    {
        public string PageName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool RequiresProxy { get; set; }
        public bool RequiresLogin { get; set; }
        public bool RequiresCustomLogin { get; set; }
        public bool RequiresRedirects { get; set; }
        public string? CredentialUsername { get; set; }
        public string? CredentialPassword { get; set; }
    }

    public class UpdatePageDto
    {
        public long PageId { get; set; }
        public string PageName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool RequiresProxy { get; set; }
        public bool RequiresLogin { get; set; }
        public bool RequiresCustomLogin { get; set; }
        public bool RequiresRedirects { get; set; }
        public string? CredentialUsername { get; set; }
        public string? CredentialPassword { get; set; }
    }
}