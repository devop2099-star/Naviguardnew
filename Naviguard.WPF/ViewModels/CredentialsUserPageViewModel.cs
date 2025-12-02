// Naviguard.WPF/ViewModels/CredentialsUserPageViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Application.DTOs;
using Naviguard.Application.Interfaces;
using Naviguard.Domain.Entities;
using Naviguard.WPF.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace Naviguard.WPF.ViewModels
{
    public partial class CredentialsUserPageViewModel : ObservableObject
    {
        private readonly IPageService _pageService;
        private readonly ICredentialService _credentialService;

        [ObservableProperty]
        private ObservableCollection<Pagina> _pagesRequiringLogin = new();

        [ObservableProperty]
        private Pagina? _selectedPage;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        public CredentialsUserPageViewModel(
            IPageService pageService,
            ICredentialService credentialService)
        {
            _pageService = pageService;
            _credentialService = credentialService;
            LoadPagesRequiringLoginAsync();
        }

        private async void LoadPagesRequiringLoginAsync()
        {
            try
            {
                var result = await _pageService.GetPagesRequiringCustomLoginAsync();

                if (result.IsSuccess && result.Value != null)
                {
                    PagesRequiringLogin.Clear();
                    foreach (var pageDto in result.Value)
                    {
                        PagesRequiringLogin.Add(new Pagina
                        {
                            PageId = pageDto.PageId,
                            PageName = pageDto.PageName,
                            Url = pageDto.Url
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar páginas: {ex.Message}", "Error");
            }
        }

        partial void OnSelectedPageChanged(Pagina? value)
        {
            if (value != null)
            {
                LoadCredentialsForPageAsync(value.PageId);
            }
        }

        private async void LoadCredentialsForPageAsync(long pageId)
        {
            if (!UserSession.IsLoggedIn) return;

            try
            {
                var result = await _credentialService.GetUserCredentialAsync(
                    UserSession.ApiUserId,
                    pageId);

                if (result.IsSuccess && result.Value != null)
                {
                    Username = result.Value.Username;
                    Password = result.Value.Password;
                    Debug.WriteLine($"Credenciales cargadas: {Username}");
                }
                else
                {
                    Username = string.Empty;
                    Password = string.Empty;
                    Debug.WriteLine("No hay credenciales guardadas");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar credenciales: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private async Task SaveCredentialsAsync()
        {
            if (SelectedPage == null)
            {
                MessageBox.Show("Seleccione una página primero", "Advertencia");
                return;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("El nombre de usuario es obligatorio", "Validación");
                return;
            }

            try
            {
                var credentialDto = new UserCredentialDto
                {
                    UserId = UserSession.ApiUserId,
                    PageId = SelectedPage.PageId,
                    Username = Username,
                    Password = Password
                };

                var result = await _credentialService.UpdateOrInsertCredentialAsync(credentialDto);

                if (result.IsSuccess)
                {
                    MessageBox.Show("Credenciales guardadas correctamente", "Éxito");
                }
                else
                {
                    MessageBox.Show(result.Error, "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private void ClearForm()
        {
            SelectedPage = null;
            Username = string.Empty;
            Password = string.Empty;
        }
    }
}