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

        [ObservableProperty]
        private ObservableCollection<Pagina> _openTabs = new();

        [ObservableProperty]
        private Pagina? _selectedTab;

        [ObservableProperty]
        private object? _currentBrowserView;

        public Action<Pagina>? OpenPageAction { get; set; }

        private readonly Dictionary<long, object> _activeBrowserViews = new(); // ✅ Usar PageId como clave

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

                    Debug.WriteLine($"[MenuNaviguardVM] Grupo '{GroupName}' cargado con {Pages.Count} páginas");

                    // Abrir automáticamente las páginas fijadas
                    foreach (var pinnedPage in PinnedPages)
                    {
                        Debug.WriteLine($"[MenuNaviguardVM] Auto-abriendo página fijada: {pinnedPage.PageName}");
                        OpenPageInTab(pinnedPage);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar páginas del grupo: {ex.Message}", "Error");
                Debug.WriteLine($"[MenuNaviguardVM] Error: {ex}");
            }
        }

        [RelayCommand]
        private void OpenPage(Pagina? page)
        {
            if (page != null)
            {
                Debug.WriteLine($"[MenuNaviguardVM] OpenPageCommand ejecutado para: {page.PageName}");
                OpenPageInTab(page);
            }
        }

        private void OpenPageInTab(Pagina page)
        {
            if (page == null) return;

            Debug.WriteLine($"[MenuNaviguardVM] OpenPageInTab llamado para: {page.PageName} (ID: {page.PageId})");

            // Verificar si ya está abierta
            if (!OpenTabs.Any(p => p.PageId == page.PageId))
            {
                // Límite de pestañas
                if (OpenTabs.Count >= 5)
                {
                    MessageBox.Show("No puedes abrir más de 5 pestañas simultáneamente.",
                        "Límite alcanzado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                OpenTabs.Add(page);
                Debug.WriteLine($"[MenuNaviguardVM] Pestaña agregada. Total: {OpenTabs.Count}");
            }
            else
            {
                Debug.WriteLine($"[MenuNaviguardVM] La pestaña ya estaba abierta");
            }

            // Seleccionar la pestaña
            SelectedTab = page;
        }

        [RelayCommand]
        private void CloseTab(Pagina? page)
        {
            if (page == null) return;

            Debug.WriteLine($"[MenuNaviguardVM] Cerrando pestaña: {page.PageName}");

            // Remover del diccionario de vistas activas
            if (_activeBrowserViews.ContainsKey(page.PageId))
            {
                _activeBrowserViews.Remove(page.PageId);
                Debug.WriteLine($"[MenuNaviguardVM] Vista removida del caché");
            }

            // Remover de pestañas abiertas
            OpenTabs.Remove(page);

            // Si era la pestaña seleccionada, seleccionar otra
            if (SelectedTab?.PageId == page.PageId)
            {
                SelectedTab = OpenTabs.FirstOrDefault();
            }
        }

        partial void OnSelectedTabChanged(Pagina? value)
        {
            if (value != null)
            {
                Debug.WriteLine($"[MenuNaviguardVM] Pestaña seleccionada: {value.PageName} (ID: {value.PageId})");

                // Verificar si ya existe una vista para esta página
                if (_activeBrowserViews.TryGetValue(value.PageId, out var existingView))
                {
                    Debug.WriteLine($"[MenuNaviguardVM] -> Usando vista existente del caché");
                    CurrentBrowserView = existingView;
                }
                else
                {
                    Debug.WriteLine($"[MenuNaviguardVM] -> Solicitando crear nueva vista del navegador");
                    // Solicitar crear nueva vista
                    OpenPageAction?.Invoke(value);
                }
            }
            else
            {
                Debug.WriteLine($"[MenuNaviguardVM] Ninguna pestaña seleccionada");
                CurrentBrowserView = null;
            }
        }

        public void RegisterBrowserView(Pagina page, object browserView)
        {
            Debug.WriteLine($"[MenuNaviguardVM] RegisterBrowserView llamado para: {page.PageName} (ID: {page.PageId})");

            if (!_activeBrowserViews.ContainsKey(page.PageId))
            {
                _activeBrowserViews[page.PageId] = browserView;
                Debug.WriteLine($"[MenuNaviguardVM] Vista registrada en caché. Total vistas: {_activeBrowserViews.Count}");
            }

            // Establecer como vista actual
            CurrentBrowserView = browserView;
            Debug.WriteLine($"[MenuNaviguardVM] CurrentBrowserView actualizado");
        }

        [RelayCommand]
        private void CloseAllTabs()
        {
            var tabsToClose = OpenTabs.ToList();
            foreach (var tab in tabsToClose)
            {
                CloseTab(tab);
            }
        }

        [RelayCommand]
        private void CloseOtherTabs()
        {
            if (SelectedTab == null) return;

            var currentTab = SelectedTab;
            var tabsToClose = OpenTabs.Where(t => t.PageId != currentTab.PageId).ToList();

            foreach (var tab in tabsToClose)
            {
                CloseTab(tab);
            }
        }
    }
}