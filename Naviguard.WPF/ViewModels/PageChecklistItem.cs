using CommunityToolkit.Mvvm.ComponentModel;
using Naviguard.Domain.Entities;

namespace Naviguard.WPF.ViewModels
{
    public partial class PageChecklistItem : ObservableObject
    {
        [ObservableProperty]
        private Pagina _page;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isPinnedInGroup;

        public PageChecklistItem(Pagina page, bool isSelected = false, bool isPinned = false)
        {
            _page = page;
            _isSelected = isSelected;
            _isPinnedInGroup = isPinned;
        }
    }
}