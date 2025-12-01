// Naviguard.WPF/Services/NavigationService.cs
using System.Windows.Controls;

namespace Naviguard.WPF.Services
{
    public class NavigationService
    {
        private ContentControl? _contentControl;

        public void Initialize(ContentControl contentControl)
        {
            _contentControl = contentControl;
        }

        public void NavigateTo(UserControl view)
        {
            if (_contentControl != null)
            {
                _contentControl.Content = view;
            }
        }

        public void NavigateTo<T>() where T : UserControl, new()
        {
            if (_contentControl != null)
            {
                _contentControl.Content = new T();
            }
        }
    }
}