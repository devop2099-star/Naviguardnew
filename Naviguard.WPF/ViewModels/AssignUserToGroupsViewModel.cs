// Naviguard.WPF/ViewModels/AssignUserToGroupsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Application.DTOs;
using Naviguard.Application.Interfaces;
using Naviguard.Domain.Entities;
using Naviguard.Domain.Interfaces;
using Naviguard.WPF.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace Naviguard.WPF.ViewModels
{
    public partial class AssignUserToGroupsViewModel : ObservableObject
    {
        private readonly IUserAssignmentService _assignmentService;
        private readonly IGroupService _groupService;
        private readonly IBusinessStructureRepository _businessRepository;

        // ✅ Propiedades de filtro
        [ObservableProperty]
        private string _filterName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<BusinessDepartment> _departments = new();

        [ObservableProperty]
        private ObservableCollection<BusinessArea> _areas = new();

        [ObservableProperty]
        private ObservableCollection<BusinessSubarea> _subareas = new();

        [ObservableProperty]
        private BusinessDepartment? _selectedDepartment;

        [ObservableProperty]
        private BusinessArea? _selectedArea;

        [ObservableProperty]
        private BusinessSubarea? _selectedSubarea;

        // ✅ Usuarios filtrados
        [ObservableProperty]
        private ObservableCollection<FilteredUser> _filteredUsers = new();

        [ObservableProperty]
        private FilteredUser? _selectedUser;

        // ✅ Búsqueda de grupos
        [ObservableProperty]
        private string _groupSearchText = string.Empty;

        // ✅ Grupos disponibles con wrapper
        [ObservableProperty]
        private ObservableCollection<GroupChecklistItem> _availableGroups = new();

        // ✅ Grupos asignados
        [ObservableProperty]
        private ObservableCollection<Group> _assignedGroups = new();

        // ✅ Lista completa de grupos (sin filtrar)
        private List<Group> _allGroups = new();

        // ✅ Propiedad computada
        public bool IsUserSelected => SelectedUser != null;

        public AssignUserToGroupsViewModel(
            IUserAssignmentService assignmentService,
            IGroupService groupService,
            IBusinessStructureRepository businessRepository)
        {
            _assignmentService = assignmentService;
            _groupService = groupService;
            _businessRepository = businessRepository;

            LoadInitialDataAsync();
        }

        private async void LoadInitialDataAsync()
        {
            await LoadDepartmentsAsync();
            await LoadGroupsAsync();
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var departments = await _businessRepository.GetDepartmentsAsync();
                Departments.Clear();
                foreach (var dept in departments)
                {
                    Departments.Add(dept);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar departamentos: {ex.Message}");
            }
        }

        private async Task LoadGroupsAsync()
        {
            try
            {
                var result = await _groupService.GetAllGroupsAsync();

                if (result.IsSuccess && result.Value != null)
                {
                    _allGroups.Clear();
                    foreach (var groupDto in result.Value)
                    {
                        _allGroups.Add(new Group
                        {
                            GroupId = groupDto.GroupId,
                            GroupName = groupDto.GroupName,
                            Description = groupDto.Description
                        });
                    }
                    RefreshAvailableGroups();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar grupos: {ex.Message}", "Error");
            }
        }

        private void RefreshAvailableGroups()
        {
            AvailableGroups.Clear();

            var filtered = string.IsNullOrWhiteSpace(GroupSearchText)
                ? _allGroups
                : _allGroups.Where(g => g.GroupName.Contains(GroupSearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var group in filtered)
            {
                AvailableGroups.Add(new GroupChecklistItem(group));
            }
        }

        // ✅ Comando de filtro
        [RelayCommand]
        private async Task FilterAsync()
        {
            try
            {
                var filterDto = new FilterUsersDto
                {
                    Name = string.IsNullOrWhiteSpace(FilterName) ? null : FilterName,
                    DepartmentId = SelectedDepartment?.DepartmentId,
                    AreaId = SelectedArea?.AreaId,
                    SubareaId = SelectedSubarea?.SubareaId
                };

                var result = await _assignmentService.FilterUsersAsync(filterDto);

                if (result.IsSuccess && result.Value != null)
                {
                    FilteredUsers.Clear();
                    foreach (var userDto in result.Value)
                    {
                        FilteredUsers.Add(new FilteredUser
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

        // ✅ Comando limpiar filtro
        [RelayCommand]
        private void ClearFilter()
        {
            FilterName = string.Empty;
            SelectedDepartment = null;
            SelectedArea = null;
            SelectedSubarea = null;
            FilteredUsers.Clear();
        }

        // ✅ Cuando cambia el departamento, cargar áreas
        partial void OnSelectedDepartmentChanged(BusinessDepartment? value)
        {
            Areas.Clear();
            SelectedArea = null;

            if (value != null)
            {
                LoadAreasAsync(value.DepartmentId);
            }
        }

        private async void LoadAreasAsync(int departmentId)
        {
            try
            {
                var areas = await _businessRepository.GetAreasByDepartmentAsync(departmentId);
                foreach (var area in areas)
                {
                    Areas.Add(area);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar áreas: {ex.Message}");
            }
        }

        // ✅ Cuando cambia el área, cargar subáreas
        partial void OnSelectedAreaChanged(BusinessArea? value)
        {
            Subareas.Clear();
            SelectedSubarea = null;

            if (value != null)
            {
                LoadSubareasAsync(value.AreaId);
            }
        }

        private async void LoadSubareasAsync(int areaId)
        {
            try
            {
                var subareas = await _businessRepository.GetSubareasByAreaAsync(areaId);
                foreach (var subarea in subareas)
                {
                    Subareas.Add(subarea);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar subáreas: {ex.Message}");
            }
        }

        // ✅ Cuando cambia el usuario seleccionado
        partial void OnSelectedUserChanged(FilteredUser? value)
        {
            OnPropertyChanged(nameof(IsUserSelected));

            if (value != null)
            {
                LoadUserGroupsAsync(value.UserId);
            }
            else
            {
                AssignedGroups.Clear();
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

        // ✅ Filtrar grupos cuando cambia el texto de búsqueda
        partial void OnGroupSearchTextChanged(string value)
        {
            RefreshAvailableGroups();
        }

        // ✅ Agregar grupos seleccionados
        [RelayCommand]
        private async Task AddSelectedGroupsAsync()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Seleccione un usuario primero", "Advertencia");
                return;
            }

            var selectedGroups = AvailableGroups
                .Where(g => g.IsSelected)
                .Select(g => g.Group.GroupId)
                .ToList();

            if (!selectedGroups.Any())
            {
                MessageBox.Show("Seleccione al menos un grupo", "Advertencia");
                return;
            }

            try
            {
                // Agregar a la lista de asignados (sin duplicados)
                foreach (var item in AvailableGroups.Where(g => g.IsSelected))
                {
                    if (!AssignedGroups.Any(ag => ag.GroupId == item.Group.GroupId))
                    {
                        AssignedGroups.Add(item.Group);
                    }
                }

                // Guardar en base de datos
                var allAssignedIds = AssignedGroups.Select(g => g.GroupId).ToList();
                var result = await _assignmentService.AssignGroupsToUserAsync(
                    SelectedUser.UserId,
                    allAssignedIds);

                if (result.IsSuccess)
                {
                    MessageBox.Show("Grupos agregados correctamente", "Éxito");

                    // Desmarcar checkboxes
                    foreach (var item in AvailableGroups)
                    {
                        item.IsSelected = false;
                    }
                }
                else
                {
                    MessageBox.Show(result.Error, "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar grupos: {ex.Message}", "Error");
            }
        }

        // ✅ Remover grupo asignado
        [RelayCommand]
        private async Task RemoveAssignedGroupAsync(Group? group)
        {
            if (group == null || SelectedUser == null) return;

            try
            {
                var result = await _assignmentService.RemoveGroupFromUserAsync(
                    SelectedUser.UserId,
                    group.GroupId);

                if (result.IsSuccess)
                {
                    AssignedGroups.Remove(group);
                    MessageBox.Show("Grupo removido correctamente", "Éxito");
                }
                else
                {
                    MessageBox.Show(result.Error, "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al remover grupo: {ex.Message}", "Error");
            }
        }

        // ✅ Comando para abrir ventana de credenciales
        [RelayCommand]
        private void OpenCredentialsWindow(FilteredUser? user)
        {
            if (user == null) return;

            try
            {
                var credentialsViewModel = App.GetService<CredentialsUserPageViewModel>();
                var credentialsWindow = new Views.Users.CredentialsUserPage(credentialsViewModel);
                credentialsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir ventana de credenciales: {ex.Message}", "Error");
            }
        }
    }

    // ✅ Clase auxiliar para grupos con checkbox
    public partial class GroupChecklistItem : ObservableObject
    {
        [ObservableProperty]
        private Group _group;

        [ObservableProperty]
        private bool _isSelected;

        public GroupChecklistItem(Group group, bool isSelected = false)
        {
            _group = group;
            _isSelected = isSelected;
        }
    }
}