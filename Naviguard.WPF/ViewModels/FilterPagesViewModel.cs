// Naviguard.WPF/ViewModels/FilterPagesViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Application.DTOs;
using Naviguard.Application.Interfaces;
using Naviguard.Domain.Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace Naviguard.WPF.ViewModels
{
    public partial class FilterPagesViewModel : ObservableObject
    {
        private readonly IPageService _pageService;
        private readonly IGroupService _groupService;

        // ✅ Propiedades de Página (que faltaban)
        [ObservableProperty]
        private string _pageName = string.Empty;

        [ObservableProperty]
        private string _pageDescription = string.Empty;

        [ObservableProperty]
        private string _pageUrl = string.Empty;

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

        // ✅ Propiedades de Grupo (que faltaban)
        [ObservableProperty]
        private string _groupName = string.Empty;

        [ObservableProperty]
        private string _groupDescription = string.Empty;

        // ✅ Colecciones
        [ObservableProperty]
        private ObservableCollection<PageListItemViewModel> _availablePages = new();

        // ✅ CONSTRUCTOR
        public FilterPagesViewModel(IPageService pageService, IGroupService groupService)
        {
            _pageService = pageService;
            _groupService = groupService;
            LoadAvailablePagesAsync();
        }

        // ✅ MÉTODO PARA CARGAR PÁGINAS
        private async void LoadAvailablePagesAsync()
        {
            try
            {
                var result = await _pageService.GetAllPagesAsync();

                if (result.IsSuccess && result.Value != null)
                {
                    AvailablePages.Clear();
                    foreach (var pageDto in result.Value)
                    {
                        AvailablePages.Add(new PageListItemViewModel(
                            new Pagina
                            {
                                PageId = pageDto.PageId,
                                PageName = pageDto.PageName,
                                Description = pageDto.Description,
                                Url = pageDto.Url
                            }
                        ));
                    }
                    Debug.WriteLine($"✅ {AvailablePages.Count} páginas cargadas");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al cargar páginas: {ex.Message}");
                MessageBox.Show($"Error al cargar páginas: {ex.Message}", "Error");
            }
        }

        // ✅ COMANDO PARA GUARDAR PÁGINA
        [RelayCommand]
        private async Task SavePageAsync()
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(PageName))
            {
                MessageBox.Show("El nombre de la página es obligatorio", "Validación");
                return;
            }

            if (string.IsNullOrWhiteSpace(PageUrl))
            {
                MessageBox.Show("La URL es obligatoria", "Validación");
                return;
            }

            if (!Uri.TryCreate(PageUrl, UriKind.Absolute, out _))
            {
                MessageBox.Show("La URL no es válida", "Validación");
                return;
            }

            try
            {
                var createDto = new CreatePageDto
                {
                    PageName = PageName,
                    Description = PageDescription,
                    Url = PageUrl,
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
                    MessageBox.Show($"Página '{PageName}' guardada con éxito", "Éxito");
                    ClearPageForm();
                    LoadAvailablePagesAsync(); // Recargar lista
                }
                else
                {
                    MessageBox.Show(result.Error, "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la página: {ex.Message}", "Error");
            }
        }

        // ✅ COMANDO PARA CREAR GRUPO
        [RelayCommand]
        private async Task CreateGroupAsync()
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(GroupName))
            {
                MessageBox.Show("El nombre del grupo es obligatorio", "Validación");
                return;
            }

            var selectedPages = AvailablePages.Where(p => p.IsSelected).ToList();
            if (selectedPages.Count == 0)
            {
                MessageBox.Show("Debes seleccionar al menos una página para el grupo", "Validación");
                return;
            }

            try
            {
                var createDto = new CreateGroupDto
                {
                    GroupName = GroupName,
                    Description = GroupDescription,
                    IsPinned = false, // Por defecto no está fijado
                    PageIds = selectedPages.Select(p => p.PageData.PageId).ToList()
                };

                var result = await _groupService.CreateGroupAsync(createDto);

                if (result.IsSuccess)
                {
                    MessageBox.Show($"Grupo '{GroupName}' creado con {selectedPages.Count} página(s)", "Éxito");
                    ClearGroupForm();
                }
                else
                {
                    MessageBox.Show(result.Error, "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear el grupo: {ex.Message}", "Error");
            }
        }

        // ✅ MÉTODO PARA LIMPIAR FORMULARIO DE PÁGINA
        private void ClearPageForm()
        {
            PageName = string.Empty;
            PageDescription = string.Empty;
            PageUrl = string.Empty;
            RequiresProxy = false;
            RequiresLogin = false;
            RequiresCustomLogin = false;
            RequiresRedirects = false;
            CredentialUsername = string.Empty;
            CredentialPassword = string.Empty;
        }

        // ✅ MÉTODO PARA LIMPIAR FORMULARIO DE GRUPO
        private void ClearGroupForm()
        {
            GroupName = string.Empty;
            GroupDescription = string.Empty;

            // Desmarcar todas las páginas
            foreach (var page in AvailablePages)
            {
                page.IsSelected = false;
            }
        }
    }

    // ✅ CLASE AUXILIAR PARA ITEMS DE LA LISTA
    public partial class PageListItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private Pagina _pageData;

        [ObservableProperty]
        private bool _isSelected;

        public string PageName => PageData.PageName;

        public PageListItemViewModel(Pagina page)
        {
            _pageData = page;
            _isSelected = false;
        }
    }
}