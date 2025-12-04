// Naviguard.WPF/ViewModels/FilterPagesViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Application.DTOs;
using Naviguard.Application.Interfaces;
using Naviguard.Domain.Entities;
using System.Collections.ObjectModel;
using System.Windows;
using DomainGroup = Naviguard.Domain.Entities.Group; // ✅ AGREGAR ALIAS

namespace Naviguard.WPF.ViewModels
{
    public partial class FilterPagesViewModel : ObservableObject
    {
        private readonly IPageService _pageService;
        private readonly IGroupService _groupService;

        [ObservableProperty]
        private ObservableCollection<Pagina> _pages = new();

        [ObservableProperty]
        private ObservableCollection<DomainGroup> _groups = new(); // ✅ USAR ALIAS

        [ObservableProperty]
        private Pagina? _selectedPage;

        [ObservableProperty]
        private string _pageName = string.Empty;

        [ObservableProperty]
        private string _url = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private bool _requiresProxy;

        [ObservableProperty]
        private bool _requiresLogin;

        [ObservableProperty]
        private bool _requiresCustomLogin;

        [ObservableProperty]
        private bool _requiresRedirects;

        [ObservableProperty]
        private string _credentialUsername = string.Empty;

        [ObservableProperty]
        private string _credentialPassword = string.Empty;

        [ObservableProperty]
        private List<DomainGroup> _selectedGroups = new(); // ✅ USAR ALIAS

        public FilterPagesViewModel(IPageService pageService, IGroupService groupService)
        {
            _pageService = pageService;
            _groupService = groupService;
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            await LoadPagesAsync();
            await LoadGroupsAsync();
        }

        private async Task LoadPagesAsync()
        {
            try
            {
                var result = await _pageService.GetAllPagesAsync();

                if (result.IsSuccess && result.Value != null)
                {
                    Pages.Clear();
                    foreach (var pageDto in result.Value)
                    {
                        Pages.Add(new Pagina
                        {
                            PageId = pageDto.PageId,
                            PageName = pageDto.PageName,
                            Url = pageDto.Url, //2
                            Description = pageDto.Description,  //1
                            RequiresProxy = pageDto.RequiresProxy,
                            RequiresLogin = pageDto.RequiresLogin,
                            RequiresCustomLogin = pageDto.RequiresCustomLogin,
                            RequiresRedirects = pageDto.RequiresRedirects
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar páginas: {ex.Message}", "Error");
            }
        }

        private async Task LoadGroupsAsync()
        {
            try
            {
                var result = await _groupService.GetAllGroupsAsync();

                if (result.IsSuccess && result.Value != null)
                {
                    Groups.Clear();
                    foreach (var groupDto in result.Value)
                    {
                        Groups.Add(new DomainGroup // ✅ USAR ALIAS
                        {
                            GroupId = groupDto.GroupId,
                            GroupName = groupDto.GroupName,
                            Description = groupDto.Description
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar grupos: {ex.Message}", "Error");
            }
        }

        partial void OnSelectedPageChanged(Pagina? value)
        {
            if (value != null)
            {
                LoadPageDetails(value);
            }
        }

        private void LoadPageDetails(Pagina page)
        {
            PageName = page.PageName;
            Url = page.Url;
            Description = page.Description ?? string.Empty;
            RequiresProxy = page.RequiresProxy;
            RequiresLogin = page.RequiresLogin;
            RequiresCustomLogin = page.RequiresCustomLogin;
            RequiresRedirects = page.RequiresRedirects;
        }

        [RelayCommand]
        private async Task CreatePageAsync()
        {
            if (string.IsNullOrWhiteSpace(PageName))
            {
                MessageBox.Show("El nombre de la página es obligatorio", "Validación");
                return;
            }

            if (string.IsNullOrWhiteSpace(Url))
            {
                MessageBox.Show("La URL es obligatoria", "Validación");
                return;
            }

            try
            {
                var createDto = new CreatePageDto
                {
                    PageName = PageName,
                    Url = Url,
                    Description = Description,
                    RequiresProxy = RequiresProxy,
                    RequiresLogin = RequiresLogin,
                    RequiresCustomLogin = RequiresCustomLogin,
                    RequiresRedirects = RequiresRedirects,
                    CredentialUsername = CredentialUsername,
                    CredentialPassword = CredentialPassword
                };

                var result = await _pageService.CreatePageAsync(createDto);

                if (result.IsSuccess)
                {
                    MessageBox.Show("Página creada correctamente", "Éxito");
                    ClearForm();
                    await LoadPagesAsync();
                }
                else
                {
                    MessageBox.Show(result.Error, "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear página: {ex.Message}", "Error");
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

            if (string.IsNullOrWhiteSpace(PageName))
            {
                MessageBox.Show("El nombre es obligatorio", "Validación");
                return;
            }

            try
            {
                var updateDto = new UpdatePageDto
                {
                    PageId = SelectedPage.PageId,
                    PageName = PageName,
                    Url = Url,
                    Description = Description,
                    RequiresProxy = RequiresProxy,
                    RequiresLogin = RequiresLogin,
                    RequiresCustomLogin = RequiresCustomLogin,
                    RequiresRedirects = RequiresRedirects,
                    CredentialUsername = CredentialUsername,
                    CredentialPassword = CredentialPassword
                };

                var result = await _pageService.UpdatePageAsync(updateDto);

                if (result.IsSuccess)
                {
                    MessageBox.Show("Página actualizada correctamente", "Éxito");
                    await LoadPagesAsync();
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
        private async Task DeletePageAsync()
        {
            if (SelectedPage == null)
            {
                MessageBox.Show("Seleccione una página primero", "Advertencia");
                return;
            }

            var confirmResult = MessageBox.Show(
                $"¿Está seguro de eliminar la página '{SelectedPage.PageName}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmResult == MessageBoxResult.Yes)
            {
                try
                {
                    var result = await _pageService.DeletePageAsync(SelectedPage.PageId);

                    if (result.IsSuccess)
                    {
                        MessageBox.Show("Página eliminada correctamente", "Éxito");
                        ClearForm();
                        await LoadPagesAsync();
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
        private void ClearForm()
        {
            SelectedPage = null;
            PageName = string.Empty;
            Url = string.Empty;
            Description = string.Empty;
            RequiresProxy = false;
            RequiresLogin = false;
            RequiresCustomLogin = false;
            RequiresRedirects = false;
            CredentialUsername = string.Empty;
            CredentialPassword = string.Empty;
        }
    }
}