using Naviguard.Domain.Common;

namespace Naviguard.Domain.Entities
{
    public class Pagina : Entity
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

        // Constructor
        public Pagina()
        {
        }

        // Métodos de dominio
        public bool IsPinnedInGroup() => PinInGroup == 1;

        public void SetPinnedInGroup(bool isPinned)
        {
            PinInGroup = isPinned ? (short)1 : (short)0;
        }

        public bool NeedsAuthentication() => RequiresLogin || RequiresCustomLogin;

        public void Activate()
        {
            State = 1;
        }

        public void Deactivate()
        {
            State = 0;
        }

        public bool IsActive() => State == 1;

        // Validaciones de dominio
        public bool IsValidUrl()
        {
            return Uri.TryCreate(Url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}