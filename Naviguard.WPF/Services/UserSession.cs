// Naviguard.WPF/Services/UserSession.cs
namespace Naviguard.WPF.Services
{
    public static class UserSession
    {
        private static long? _apiUserId;
        private static string? _userName;

        public static long ApiUserId
        {
            get
            {
                if (!_apiUserId.HasValue)
                    throw new InvalidOperationException("No hay sesión activa (ApiUserId).");
                return _apiUserId.Value;
            }
        }

        public static string UserName
        {
            get
            {
                if (string.IsNullOrEmpty(_userName))
                    throw new InvalidOperationException("Nombre de usuario no disponible en la sesión.");
                return _userName;
            }
        }

        public static bool IsLoggedIn => _apiUserId.HasValue;

        public static void StartSession(long apiUserId, string userName)
        {
            _apiUserId = apiUserId;
            _userName = userName;
        }

        public static void EndSession()
        {
            _apiUserId = null;
            _userName = null;
        }
    }
}