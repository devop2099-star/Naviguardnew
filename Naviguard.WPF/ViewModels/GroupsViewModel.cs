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
            if (!UserSession.IsLoggedIn)
            {
                Debug.WriteLine("No hay sesión activa");
                return;
            }

            try
            {
                long userId = UserSession.ApiUserId;
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
                }
                else
                {
                    MessageBox.Show(result.Error, "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar grupos: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private void OpenGroup(Group group)
        {
            if (group == null) return;
            Debug.WriteLine($"[GroupsPagesViewModel] Abriendo grupo: '{group.GroupName}', ID: {group.GroupId}");
            NavigateToGroupAction?.Invoke(group);
        }
    }
}