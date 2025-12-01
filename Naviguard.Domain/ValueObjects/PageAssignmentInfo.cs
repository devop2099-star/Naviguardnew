namespace Naviguard.Domain.ValueObjects
{
    public class PageAssignmentInfo
    {
        public long PageId { get; set; }
        public bool IsPinned { get; set; }

        public PageAssignmentInfo()
        {
        }

        public PageAssignmentInfo(long pageId, bool isPinned)
        {
            PageId = pageId;
            IsPinned = isPinned;
        }
    }
}