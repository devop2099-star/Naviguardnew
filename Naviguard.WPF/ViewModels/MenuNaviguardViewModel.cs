// Naviguard.WPF/ViewModels/MenuNaviguardViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Application.Interfaces;
using Naviguard.Domain.Entities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace Naviguard.WPF.ViewModels
{
    public partial class MenuNaviguardViewModel : ObservableObject
    {
        private readonly IGroupService _groupService;
        private readonly long _groupId;

        [ObservableProperty]
        private string _groupName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Pagina> _pages = new();

        [ObservableProperty]
        private ObservableCollection<Pagina> _pinnedPages = new();

        [ObservableProperty]
        private ObservableCollection<Pagina> _unpinnedPages = new();

        public Action<Pagina>? OpenPageAction { get; set; }

        public MenuNaviguardViewModel(IGroupService groupService, long groupId)
        {
            _groupService = groupService;
            _groupId = groupId;
            LoadGroupPagesAsync();
        }

        private async void LoadGroupPagesAsync()
        {
            try
            {
                var result = await _groupService.GetGroupByIdAsync(_groupId);

                if (result.IsSuccess && result.Value != null)
                {
                    GroupName = result.Value.GroupName;

                    Pages.Clear();
                    PinnedPages.Clear();
                    UnpinnedPages.Clear();

                    foreach (var pageDto in result.Value.Pages)
                    {
                        var page = new Pagina
                        {
                            PageId = pageDto.PageId,
                            PageName = pageDto.PageName,
                            Url = pageDto.Url,
                            RequiresProxy = pageDto.RequiresProxy,
                            RequiresLogin = pageDto.RequiresLogin,
                            RequiresCustomLogin = pageDto.RequiresCustomLogin,
                            RequiresRedirects = pageDto.RequiresRedirects,
                            PinInGroup = pageDto.PinInGroup
                        };

                        Pages.Add(page);

                        if (page.PinInGroup == 1)
                            PinnedPages.Add(page);
                        else
                            UnpinnedPages.Add(page);
                    }

                    Debug.WriteLine($"Grupo '{GroupName}' cargado con {Pages.Count} páginas");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar páginas del grupo: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private void OpenPage(Pagina? page)
        {
            if (page != null)
            {
                Debug.WriteLine($"Abriendo página: {page.PageName}");
                OpenPageAction?.Invoke(page);
            }
        }
    }
}