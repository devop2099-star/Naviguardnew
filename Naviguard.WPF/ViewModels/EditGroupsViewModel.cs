using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Application.DTOs;
using Naviguard.Application.Interfaces;
using Naviguard.Domain.Entities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace Naviguard.WPF.ViewModels
{
    public partial class EditGroupsViewModel : ObservableObject
    {
        private readonly IGroupService _groupService;
        private readonly IPageService _pageService;

        // Colecciones originales
        private List<Group> _allGroups = new();
        private List<Pagina> _allPages = new();

        // Colecciones filtradas
        [ObservableProperty]
        private ObservableCollection<Group> _filteredGroups = new();

        [ObservableProperty]
        private ObservableCollection<Pagina> _filteredPages = new();

        // Selección
        [ObservableProperty]
        private Group? _selectedGroup;

        [ObservableProperty]
        private Pagina? _selectedPage;

        // Modo de edición (true = páginas, false = grupos)
        [ObservableProperty]
        private bool _isEditingPages;

        // Búsqueda
        [ObservableProperty]
        private string _searchText = string.Empty;

        // Propiedades de grupo
        [ObservableProperty]
        private string _editGroupName = string.Empty;

        [ObservableProperty]
        private string _editGroupDescription = string.Empty;

        [ObservableProperty]
        private bool _editGroupIsPinned;

        [ObservableProperty]
        private ObservableCollection<PageChecklistItem> _allPagesChecklist = new();

        // Propiedades de página
        [ObservableProperty]
        private string _editPageName = string.Empty;

        [ObservableProperty]
        private string _editPageDescription = string.Empty;

        [ObservableProperty]
        private string _editPageUrl = string.Empty;

        [ObservableProperty]
        private bool _editRequiresProxy;

        [ObservableProperty]
        private bool _editRequiresLogin;

        [ObservableProperty]
        private bool _editRequiresCustomLogin;

        [ObservableProperty]
        private bool _editRequiresRedirects;

        [ObservableProperty]
        private string _editCredentialUsername = string.Empty;

        [ObservableProperty]
        private string _editCredentialPassword = string.Empty;

        // Propiedades computadas
        public bool IsGroupSelected => SelectedGroup != null && !IsEditingPages;
        public bool IsPageSelected => SelectedPage != null && IsEditingPages;

        public EditGroupsViewModel(IGroupService groupService, IPageService pageService)
        {
            _groupService = groupService;
            _pageService = pageService;
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            await LoadGroupsAsync();
            await LoadPagesAsync();
        }

        private async Task LoadGroupsAsync()
        {
            try
            {
                var result = await _groupService.GetGroupsWithPagesAsync();

                if (result.IsSuccess && result.Value != null)
                {
                    _allGroups.Clear();
                    foreach (var groupDto in result.Value)
                    {
                        var group = new Group
                        {
                            GroupId = groupDto.GroupId,
                            GroupName = groupDto.GroupName,
                            Description = groupDto.Description,
                            Pin = groupDto.Pin,
                            Pages = new List<Pagina>()
                        };

                        foreach (var pageDto in groupDto.Pages)
                        {
                            group.Pages.Add(new Pagina
                            {
                                PageId = pageDto.PageId,
                                PageName = pageDto.PageName,
                                Url = pageDto.Url,
                                PinInGroup = pageDto.PinInGroup
                            });
                        }

                        _allGroups.Add(group);
                    }

                    FilterGroups();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar grupos: {ex.Message}", "Error");
            }
        }

        private async Task LoadPagesAsync()
        {
            try
            {
                var result = await _pageService.GetAllPagesAsync();

                if (result.IsSuccess && result.Value != null)
                {
                    _allPages.Clear();
                    foreach (var pageDto in result.Value)
                    {
                        _allPages.Add(new Pagina
                        {
                            PageId = pageDto.PageId,
                            PageName = pageDto.PageName,
                            Url = pageDto.Url,
                            Description = pageDto.Description,
                            RequiresProxy = pageDto.RequiresProxy,
                            RequiresLogin = pageDto.RequiresLogin,
                            RequiresCustomLogin = pageDto.RequiresCustomLogin,
                            RequiresRedirects = pageDto.RequiresRedirects
                        });
                    }

                    FilterPages();
                    RefreshPagesChecklist();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar páginas: {ex.Message}", "Error");
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            if (IsEditingPages)
                FilterPages();
            else
                FilterGroups();
        }

        private void FilterGroups()
        {
            FilteredGroups.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allGroups
                : _allGroups.Where(g => g.GroupName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var group in filtered)
            {
                FilteredGroups.Add(group);
            }
        }

        private void FilterPages()
        {
            FilteredPages.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allPages
                : _allPages.Where(p => p.PageName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var page in filtered)
            {
                FilteredPages.Add(page);
            }
        }

        partial void OnSelectedGroupChanged(Group? value)
        {
            if (value != null && !IsEditingPages)
            {
                LoadGroupDetails(value);
            }
            OnPropertyChanged(nameof(IsGroupSelected));
        }

        partial void OnSelectedPageChanged(Pagina? value)
        {
            if (value != null && IsEditingPages)
            {
                LoadPageDetails(value);
            }
            OnPropertyChanged(nameof(IsPageSelected));
        }

        partial void OnIsEditingPagesChanged(bool value)
        {
            OnPropertyChanged(nameof(IsGroupSelected));
            OnPropertyChanged(nameof(IsPageSelected));
        }

        private void LoadGroupDetails(Group group)
        {
            EditGroupName = group.GroupName;
            EditGroupDescription = group.Description ?? string.Empty;
            EditGroupIsPinned = group.Pin == 1;

            RefreshPagesChecklist();
        }

        private void RefreshPagesChecklist()
        {
            AllPagesChecklist.Clear();

            if (SelectedGroup != null)
            {
                var assignedPageIds = SelectedGroup.Pages.Select(p => p.PageId).ToHashSet();

                foreach (var page in _allPages)
                {
                    bool isAssigned = assignedPageIds.Contains(page.PageId);
                    bool isPinned = false;

                    if (isAssigned)
                    {
                        var assignedPage = SelectedGroup.Pages.FirstOrDefault(p => p.PageId == page.PageId);
                        isPinned = assignedPage?.PinInGroup == 1;
                    }

                    AllPagesChecklist.Add(new PageChecklistItem(page, isAssigned, isPinned));
                }
            }
            else
            {
                foreach (var page in _allPages)
                {
                    AllPagesChecklist.Add(new PageChecklistItem(page, false, false));
                }
            }
        }

        private void LoadPageDetails(Pagina page)
        {
            EditPageName = page.PageName;
            EditPageDescription = page.Description ?? string.Empty;
            EditPageUrl = page.Url;
            EditRequiresProxy = page.RequiresProxy;
            EditRequiresLogin = page.RequiresLogin;
            EditRequiresCustomLogin = page.RequiresCustomLogin;
            EditRequiresRedirects = page.RequiresRedirects;

            // Cargar credenciales si existen
            LoadPageCredentialsAsync(page.PageId);
        }

        private async void LoadPageCredentialsAsync(long pageId)
        {
            try
            {
                // Aquí deberías llamar a un servicio para obtener las credenciales
                // Por ahora lo dejamos vacío
                EditCredentialUsername = string.Empty;
                EditCredentialPassword = string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar credenciales: {ex.Message}");
            }
        }

        [RelayCommand]
        private void TogglePinPageInGroup(PageChecklistItem? item)
        {
            if (item != null && item.IsSelected)
            {
                item.IsPinnedInGroup = !item.IsPinnedInGroup;
            }
        }

        [RelayCommand]
        private async Task UpdateGroupAsync()
        {
            if (SelectedGroup == null)
            {
                MessageBox.Show("Seleccione un grupo primero", "Advertencia");
                return;
            }

            if (string.IsNullOrWhiteSpace(EditGroupName))
            {
                MessageBox.Show("El nombre del grupo es obligatorio", "Validación");
                return;
            }

            try
            {
                var selectedPages = AllPagesChecklist
                    .Where(p => p.IsSelected)
                    .Select(p => new PageAssignmentDto
                    {
                        PageId = p.Page.PageId,
                        IsPinned = p.IsPinnedInGroup
                    })
                    .ToList();

                var updateDto = new UpdateGroupDto
                {
                    GroupId = SelectedGroup.GroupId,
                    GroupName = EditGroupName,
                    Description = EditGroupDescription,
                    Pin = (short)(EditGroupIsPinned ? 1 : 0),
                    Pages = selectedPages
                };

                var result = await _groupService.UpdateGroupAsync(updateDto);

                if (result.IsSuccess)
                {
                    MessageBox.Show("Grupo actualizado correctamente", "Éxito");
                    await LoadGroupsAsync();
                    SelectedGroup = _allGroups.FirstOrDefault(g => g.GroupId == updateDto.GroupId);
                }
                else
                {
                    MessageBox.Show(result.Error, "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private async Task UpdatePageAsync()
        {
            if (SelectedPage == null)
            {
                MessageBox.Show("Seleccione una página primero", "Advertencia");
                return;
            }

            if (string.IsNullOrWhiteSpace(EditPageName))
            {
                MessageBox.Show("El nombre es obligatorio", "Validación");
                return;
            }

            try
            {
                var updateDto = new UpdatePageDto
                {
                    PageId = SelectedPage.PageId,
                    PageName = EditPageName,
                    Url = EditPageUrl,
                    Description = EditPageDescription,
                    RequiresProxy = EditRequiresProxy,
                    RequiresLogin = EditRequiresLogin,
                    RequiresCustomLogin = EditRequiresCustomLogin,
                    RequiresRedirects = EditRequiresRedirects,
                    CredentialUsername = EditCredentialUsername,
                    CredentialPassword = EditCredentialPassword
                };

                var result = await _pageService.UpdatePageAsync(updateDto);

                if (result.IsSuccess)
                {
                    MessageBox.Show("Página actualizada correctamente", "Éxito");
                    await LoadPagesAsync();
                    SelectedPage = _allPages.FirstOrDefault(p => p.PageId == updateDto.PageId);
                }
                else
                {
                    MessageBox.Show(result.Error, "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private async Task DeleteGroupAsync(Group? group)
        {
            if (group == null) return;

            var confirmResult = MessageBox.Show(
                $"¿Está seguro de eliminar el grupo '{group.GroupName}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmResult == MessageBoxResult.Yes)
            {
                try
                {
                    var result = await _groupService.DeleteGroupAsync(group.GroupId);

                    if (result.IsSuccess)
                    {
                        MessageBox.Show("Grupo eliminado correctamente", "Éxito");
                        SelectedGroup = null;
                        await LoadGroupsAsync();
                    }
                    else
                    {
                        MessageBox.Show(result.Error, "Error");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar: {ex.Message}", "Error");
                }
            }
        }

        [RelayCommand]
        private async Task DeletePageAsync(Pagina? page)
        {
            if (page == null) return;

            var confirmResult = MessageBox.Show(
                $"¿Está seguro de eliminar la página '{page.PageName}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmResult == MessageBoxResult.Yes)
            {
                try
                {
                    var result = await _pageService.DeletePageAsync(page.PageId);

                    if (result.IsSuccess)
                    {
                        MessageBox.Show("Página eliminada correctamente", "Éxito");
                        SelectedPage = null;
                        await LoadPagesAsync();
                        await LoadGroupsAsync(); // Recargar grupos también
                    }
                    else
                    {
                        MessageBox.Show(result.Error, "Error");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar: {ex.Message}", "Error");
                }
            }
        }
    }
}