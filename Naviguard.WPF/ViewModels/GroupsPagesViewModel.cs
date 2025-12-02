using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Application.Interfaces;
using Naviguard.Domain.Entities;
using Naviguard.WPF.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace Naviguard.WPF.ViewModels
{
    public partial class GroupsPagesViewModel : ObservableObject
    {
        private readonly IUserAssignmentService _assignmentService;

        [ObservableProperty]
        private ObservableCollection<Group> _grupos = new();

        public Action<Group>? NavigateToGroupAction { get; set; }

        public GroupsPagesViewModel(IUserAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
            LoadUserGroupsAsync();
        }

        private async void LoadUserGroupsAsync()
        {
            try
            {
                // ✅ Validación de sesión
                if (!UserSession.IsLoggedIn)
                {
                    Debug.WriteLine("❌ No hay sesión activa");
                    MessageBox.Show("No hay una sesión activa. Por favor, inicie sesión.", "Error de Sesión");
                    return;
                }

                long userId = UserSession.ApiUserId;
                Debug.WriteLine($"🔍 Cargando grupos para el usuario ID: {userId}");

                var result = await _assignmentService.GetGroupsByUserIdAsync((int)userId);

                if (result.IsSuccess && result.Value != null)
                {
                    Grupos.Clear();
                    foreach (var groupDto in result.Value)
                    {
                        Grupos.Add(new Group
                        {
                            GroupId = groupDto.GroupId,
                            GroupName = groupDto.GroupName,
                            Description = groupDto.Description,
                            Pin = groupDto.Pin
                        });
                    }

                    Debug.WriteLine($"✅ {Grupos.Count} grupos cargados correctamente");
                }
                else
                {
                    Debug.WriteLine($"❌ Error al cargar grupos: {result.Error}");
                    MessageBox.Show(result.Error, "Error al cargar grupos");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Excepción al cargar grupos: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                MessageBox.Show($"Error al cargar grupos: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private void OpenGroup(Group? group)
        {
            if (group == null)
            {
                Debug.WriteLine("❌ El grupo es null");
                return;
            }

            Debug.WriteLine($"[GroupsPagesViewModel] Abriendo grupo: '{group.GroupName}', ID: {group.GroupId}");

            if (NavigateToGroupAction == null)
            {
                Debug.WriteLine("❌ NavigateToGroupAction es null");
                MessageBox.Show("No se puede navegar. Contacte al administrador.", "Error");
                return;
            }

            NavigateToGroupAction.Invoke(group);
        }
    }
}