using Naviguard.Domain.Common;

namespace Naviguard.Domain.Entities
{
    public class Group : Entity
    {
        public long GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public short Pin { get; set; }

        // Navegación (colecciones)
        public ICollection<Pagina> Pages { get; set; } = new List<Pagina>();
        public HashSet<long> PinnedPageIds { get; set; } = new();

        // Constructor
        public Group()
        {
            Pages = new List<Pagina>();
            PinnedPageIds = new HashSet<long>();
        }

        // Métodos de dominio (lógica de negocio)
        public bool IsPinned() => Pin == 1;

        public void SetPinned(bool isPinned)
        {
            Pin = isPinned ? (short)1 : (short)0;
        }

        public void AddPage(Pagina page)
        {
            if (!Pages.Any(p => p.PageId == page.PageId))
            {
                Pages.Add(page);
            }
        }

        public void RemovePage(long pageId)
        {
            var page = Pages.FirstOrDefault(p => p.PageId == pageId);
            if (page != null)
            {
                Pages.Remove(page);
                PinnedPageIds.Remove(pageId);
            }
        }

        public void PinPage(long pageId)
        {
            if (Pages.Any(p => p.PageId == pageId))
            {
                PinnedPageIds.Add(pageId);
            }
        }

        public void UnpinPage(long pageId)
        {
            PinnedPageIds.Remove(pageId);
        }
    }
}