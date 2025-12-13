namespace Naviguard.Domain.Entities
{
    public class FilteredUser
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;

        public FilteredUser()
        {
        }

        public FilteredUser(int userId, string fullName)
        {
            UserId = userId;
            FullName = fullName;
        }
        public override string ToString()
        {
            return FullName;
        }
    }
}