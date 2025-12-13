using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Domain.Entities;
using Naviguard.Domain.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace Naviguard.WPF.ViewModels
{
    public partial class EditCustomLoginViewModel : ObservableObject
    {
        private readonly IPageRepository _pageRepository;
        private readonly ICredentialRepository _credentialRepository;
        private readonly IBusinessStructureRepository _businessStructureRepository;

        // Colecciones
        private List<Pagina> _allPages = new();
        
        [ObservableProperty]
        private ObservableCollection<Pagina> _filteredPages = new();

        [ObservableProperty]
        private ObservableCollection<UserPageCredential> _pageCredentials = new();

        private List<UserPageCredential> _allCredentials = new();

        // Selección
        private Pagina? _selectedPage;
        public Pagina? SelectedPage
        {
            get => _selectedPage;
            set
            {
                if (SetProperty(ref _selectedPage, value))
                {
                    OnSelectedPageChanged(value);
                }
            }
        }

        [ObservableProperty]
        private UserPageCredential? _selectedCredential;

        // Búsqueda
        [ObservableProperty]
        private string _searchText = string.Empty;

        // Edición de Credencial
        [ObservableProperty]
        private string _editUserId = string.Empty; // ID externo (ej. 251)

        [ObservableProperty]
        private string _editUsername = string.Empty; // Usuario para logearse

        [ObservableProperty]
        private string _editPassword = string.Empty; // Contraseña para logearse

        [ObservableProperty]
        private bool _isEditingCredential;

        // Búsqueda de Usuarios para Agregar
        [ObservableProperty]
        private string _userSearchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<FilteredUser> _foundUsers = new();

        private FilteredUser? _selectedFoundUser;
        public FilteredUser? SelectedFoundUser
        {
            get => _selectedFoundUser;
            set
            {
                if (SetProperty(ref _selectedFoundUser, value))
                {
                    if (value != null)
                    {
                        EditUserId = value.UserId.ToString();
                        
                        // Verificar si el usuario ya tiene credenciales en la lista actual
                        var existingCred = PageCredentials.FirstOrDefault(c => c.ExternalUserId == value.UserId);
                        if (existingCred != null)
                        {
                            EditUsername = existingCred.Username;
                            EditPassword = existingCred.Password;
                        }
                        else
                        {
                            EditUsername = string.Empty;
                            EditPassword = string.Empty;
                        }
                    }
                }
            }
        }

        public EditCustomLoginViewModel(
            IPageRepository pageRepository, 
            ICredentialRepository credentialRepository,
            IBusinessStructureRepository businessStructureRepository)
        {
            _pageRepository = pageRepository;
            _credentialRepository = credentialRepository;
            _businessStructureRepository = businessStructureRepository;
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            await LoadPagesAsync();
        }

        private async Task LoadPagesAsync()
        {
            try
            {
                var result = await _pageRepository.GetPagesRequiringCustomLoginAsync();
                
                _allPages = result;
                FilterPages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar páginas: {ex.Message}", "Error");
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterPages();
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

        private void OnSelectedPageChanged(Pagina? page)
        {
            if (page != null)
            {
                LoadCredentialsForPage(page.PageId);
            }
            else
            {
                PageCredentials.Clear();
            }
            
            // Limpiar campos de edición
            ClearEditFields();
        }

        private async void LoadCredentialsForPage(long pageId)
        {
            try
            {
                PageCredentials.Clear();
                _allCredentials.Clear();
                var credentials = await _credentialRepository.GetUsersForPageAsync(pageId);

                // Obtener nombres de personas
                var userIds = credentials.Select(c => c.ExternalUserId).Distinct().ToList();
                var usersInfo = await _businessStructureRepository.GetUsersByIdsAsync(userIds);

                foreach (var cred in credentials)
                {
                    var info = usersInfo.FirstOrDefault(u => u.UserId == cred.ExternalUserId);
                    if (info != null)
                    {
                        cred.PersonName = info.FullName;
                        _allCredentials.Add(cred); // Agregar a la lista completa
                    }
                    // Si no se encuentra (Ext ID...), no se agrega
                }
                
                UpdateCredentialFilter(); // Aplicar filtro inicial (vacío)
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}", "Error");
            }
        }

        private void UpdateCredentialFilter()
        {
            PageCredentials.Clear();
            var searchText = UserSearchText?.Trim();

            var filtered = string.IsNullOrWhiteSpace(searchText)
                ? _allCredentials
                : _allCredentials.Where(c => c.PersonName.Contains(searchText, StringComparison.OrdinalIgnoreCase));

            foreach (var cred in filtered)
            {
                PageCredentials.Add(cred);
            }
        }



        partial void OnSelectedCredentialChanged(UserPageCredential? value)
        {
            if (value != null)
            {
                EditUserId = value.ExternalUserId.ToString();
                EditUsername = value.Username;
                EditPassword = value.Password;
                IsEditingCredential = true;
            }
            else
            {
                ClearEditFields();
            }
        }

        private void ClearEditFields()
        {
            EditUserId = string.Empty;
            EditUsername = string.Empty;
            EditPassword = string.Empty;
            IsEditingCredential = false;
            SelectedCredential = null;
            
            // Limpiar búsqueda
            UserSearchText = string.Empty;
            FoundUsers.Clear();
            SelectedFoundUser = null;
        }

        [RelayCommand]
        private async Task SearchUserAsync()
        {
            UpdateCredentialFilter(); // Filtrar localmente al presionar buscar
            
            if (string.IsNullOrWhiteSpace(UserSearchText)) return;

            try
            {
                var users = await _businessStructureRepository.FilterUsersAsync(UserSearchText, null, null, null);
                FoundUsers.Clear();
                foreach (var user in users)
                {
                    FoundUsers.Add(user);
                }

                if (!FoundUsers.Any())
                {
                    MessageBox.Show("No se encontraron usuarios con ese nombre.", "Búsqueda");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar usuario: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private void ClearSearch()
        {
            UserSearchText = string.Empty;
            FoundUsers.Clear();
            SelectedFoundUser = null;
            UpdateCredentialFilter(); // Restablecer lista completa
        }



        [RelayCommand]
        private async Task SaveCredentialAsync()
        {
            if (SelectedPage == null)
            {
                MessageBox.Show("Seleccione una página primero.", "Validación");
                return;
            }

            if (string.IsNullOrWhiteSpace(EditUserId) || !long.TryParse(EditUserId, out long userId))
            {
                MessageBox.Show("Ingrese un ID de Usuario válido (Numérico).", "Validación");
                return;
            }

            if (string.IsNullOrWhiteSpace(EditUsername) || string.IsNullOrWhiteSpace(EditPassword))
            {
                MessageBox.Show("Usuario y contraseña son obligatorios.", "Validación");
                return;
            }

            try
            {
                await _credentialRepository.UpdateOrInsertCredentialAsync(
                    userId, 
                    SelectedPage.PageId, 
                    EditUsername, 
                    EditPassword);

                MessageBox.Show("Credencial guardada correctamente.", "Éxito");
                
                // Recargar lista
                LoadCredentialsForPage(SelectedPage.PageId);
                ClearEditFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private async Task DeleteCredentialAsync(UserPageCredential? credential)
        {
            if (credential == null) return;

            var confirm = MessageBox.Show(
                $"¿Seguro de eliminar acceso para el usuario ID {credential.ExternalUserId}?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    await _credentialRepository.DeleteCredentialAsync(credential.ExternalUserId, credential.PageId);
                    
                    MessageBox.Show("Eliminado correctamente.", "Éxito");
                    LoadCredentialsForPage(credential.PageId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar: {ex.Message}", "Error");
                }
            }
        }
    }
}
