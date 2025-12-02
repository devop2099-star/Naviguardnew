using System.Diagnostics;

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
                {
                    Debug.WriteLine("❌ ERROR: No hay sesión activa (ApiUserId es null)");
                    throw new InvalidOperationException("No hay sesión activa (ApiUserId).");
                }
                return _apiUserId.Value;
            }
        }

        public static string UserName
        {
            get
            {
                if (string.IsNullOrEmpty(_userName))
                {
                    Debug.WriteLine("❌ ERROR: Nombre de usuario no disponible en la sesión");
                    throw new InvalidOperationException("Nombre de usuario no disponible en la sesión.");
                }
                return _userName;
            }
        }

        public static bool IsLoggedIn => _apiUserId.HasValue;

        public static void StartSession(long apiUserId, string userName)
        {
            _apiUserId = apiUserId;
            _userName = userName;
            Debug.WriteLine($"✅ Sesión iniciada - UserID: {apiUserId}, UserName: {userName}");
        }

        public static void EndSession()
        {
            Debug.WriteLine($"🔚 Cerrando sesión - UserID: {_apiUserId}, UserName: {_userName}");
            _apiUserId = null;
            _userName = null;
        }
    }
}