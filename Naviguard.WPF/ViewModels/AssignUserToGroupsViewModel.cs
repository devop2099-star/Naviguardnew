// Naviguard.WPF/ViewModels/AssignUserToGroupsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Application.DTOs;
using Naviguard.Application.Interfaces;
using Naviguard.Domain.Entities;
using System.Collections.ObjectModel;
using System.Windows;

namespace Naviguard.WPF.ViewModels
{
    public partial class AssignUserToGroupsViewModel : ObservableObject
    {
        private readonly IUserAssignmentService _assignmentService;
        private readonly IGroupService _groupService;

        [ObservableProperty]
        private ObservableCollection<BusinessDepartment> _departments = new();

        [ObservableProperty]
        private ObservableCollection<BusinessArea> _areas = new();

        [ObservableProperty]
        private ObservableCollection<BusinessSubarea> _subareas = new();

        [ObservableProperty]
        private ObservableCollection<FilteredUser> _users = new();

        [ObservableProperty]
        private ObservableCollection<Group> _availableGroups = new();

        [ObservableProperty]
        private ObservableCollection<Group> _assignedGroups = new();

        [ObservableProperty]
        private BusinessDepartment? _selectedDepartment;

        [ObservableProperty]
        private BusinessArea? _selectedArea;

        [ObservableProperty]
        private BusinessSubarea? _selectedSubarea;

        [ObservableProperty]
        private FilteredUser? _selectedUser;

        [ObservableProperty]
        private string _searchName = string.Empty;

        public AssignUserToGroupsViewModel(
            IUserAssignmentService assignmentService,
            IGroupService groupService)
        {
            _assignmentService = assignmentService;
            _groupService = groupService;
            LoadInitialDataAsync();
        }

        private async void LoadInitialDataAsync()
        {
            await LoadGroupsAsync();
        }

        private async Task LoadGroupsAsync()
        {
            try
            {
                var result = await _groupService.GetAllGroupsAsync();

                if (result.IsSuccess && result.Value != null)
                {
                    AvailableGroups.Clear();
                    foreach (var groupDto in result.Value)
                    {
                        AvailableGroups.Add(new Group
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

        [RelayCommand]
        private async Task SearchUsersAsync()
        {
            try
            {
                var filterDto = new FilterUsersDto
                {
                    Name = string.IsNullOrWhiteSpace(SearchName) ? null : SearchName,
                    DepartmentId = SelectedDepartment?.DepartmentId,
                    AreaId = SelectedArea?.AreaId,
                    SubareaId = SelectedSubarea?.SubareaId
                };

                var result = await _assignmentService.FilterUsersAsync(filterDto);

                if (result.IsSuccess && result.Value != null)
                {
                    Users.Clear();
                    foreach (var userDto in result.Value)
                    {
                        Users.Add(new FilteredUser
                        {
                            UserId = userDto.UserId,
                            FullName = userDto.FullName
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
                MessageBox.Show($"Error al buscar usuarios: {ex.Message}", "Error");
            }
        }

        partial void OnSelectedUserChanged(FilteredUser? value)
        {
            if (value != null)
            {
                LoadUserGroupsAsync(value.UserId);
            }
        }

        private async void LoadUserGroupsAsync(int userId)
        {
            try
            {
                var result = await _assignmentService.GetGroupsByUserIdAsync(userId);

                if (result.IsSuccess && result.Value != null)
                {
                    AssignedGroups.Clear();
                    foreach (var groupDto in result.Value)
                    {
                        AssignedGroups.Add(new Group
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
                MessageBox.Show($"Error al cargar grupos del usuario: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private void AddGroupToUser(Group? group)
        {
            if (group != null && !AssignedGroups.Any(g => g.GroupId == group.GroupId))
            {
                AssignedGroups.Add(group);
            }
        }

        [RelayCommand]
        private void RemoveGroupFromUser(Group? group)
        {
            if (group != null)
            {
                AssignedGroups.Remove(group);
            }
        }

        [RelayCommand]
        private async Task SaveAssignmentsAsync()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Seleccione un usuario primero", "Advertencia");
                return;
            }

            try
            {
                var groupIds = AssignedGroups.Select(g => g.GroupId).ToList();
                var result = await _assignmentService.AssignGroupsToUserAsync(
                    SelectedUser.UserId,
                    groupIds);

                if (result.IsSuccess)
                {
                    MessageBox.Show("Asignaciones guardadas correctamente", "Éxito");
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
    }
}