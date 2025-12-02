// Naviguard.WPF/ViewModels/EditGroupsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Application.DTOs;
using Naviguard.Application.Interfaces;
using Naviguard.Domain.Entities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Naviguard.WPF.ViewModels
{
    public partial class EditGroupsViewModel : ObservableObject
    {
        private readonly IGroupService _groupService;
        private readonly IPageService _pageService;

        [ObservableProperty]
        private ObservableCollection<Group> _grupos = new();

        [ObservableProperty]
        private ObservableCollection<Pagina> _paginasDisponibles = new();

        [ObservableProperty]
        private ObservableCollection<Pagina> _paginasAsignadas = new();

        [ObservableProperty]
        private Group? _selectedGroup;

        [ObservableProperty]
        private string _groupName = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private bool _isPinned;

        public EditGroupsViewModel(IGroupService groupService, IPageService pageService)
        {
            _groupService = groupService;
            _pageService = pageService;
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            await LoadGroupsAsync();
            await LoadAllPagesAsync();
        }

        private async Task LoadGroupsAsync()
        {
            try
            {
                var result = await _groupService.GetGroupsWithPagesAsync();

                if (result.IsSuccess && result.Value != null)
                {
                    Grupos.Clear();
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

                        Grupos.Add(group);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar grupos: {ex.Message}", "Error");
            }
        }

        private async Task LoadAllPagesAsync()
        {
            try
            {
                var result = await _pageService.GetAllPagesAsync();

                if (result.IsSuccess && result.Value != null)
                {
                    PaginasDisponibles.Clear();
                    foreach (var pageDto in result.Value)
                    {
                        PaginasDisponibles.Add(new Pagina
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

        partial void OnSelectedGroupChanged(Group? value)
        {
            if (value != null)
            {
                LoadGroupDetails(value);
            }
        }

        private void LoadGroupDetails(Group group)
        {
            GroupName = group.GroupName;
            Description = group.Description ?? string.Empty;
            IsPinned = group.Pin == 1;

            PaginasAsignadas.Clear();
            foreach (var page in group.Pages)
            {
                PaginasAsignadas.Add(page);
            }
        }

        [RelayCommand]
        private void AddPageToGroup(Pagina? page)
        {
            if (page != null && !PaginasAsignadas.Any(p => p.PageId == page.PageId))
            {
                PaginasAsignadas.Add(page);
            }
        }

        [RelayCommand]
        private void RemovePageFromGroup(Pagina? page)
        {
            if (page != null)
            {
                PaginasAsignadas.Remove(page);
            }
        }

        [RelayCommand]
        private void TogglePagePin(Pagina? page)
        {
            if (page != null)
            {
                page.PinInGroup = (short)(page.PinInGroup == 1 ? 0 : 1);
                OnPropertyChanged(nameof(PaginasAsignadas));
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

            if (string.IsNullOrWhiteSpace(GroupName))
            {
                MessageBox.Show("El nombre del grupo es obligatorio", "Validación");
                return;
            }

            try
            {
                var updateDto = new UpdateGroupDto
                {
                    GroupId = SelectedGroup.GroupId,
                    GroupName = GroupName,
                    Description = Description,
                    Pin = (short)(IsPinned ? 1 : 0),
                    Pages = PaginasAsignadas.Select(p => new PageAssignmentDto
                    {
                        PageId = p.PageId,
                        IsPinned = p.PinInGroup == 1
                    }).ToList()
                };

                var result = await _groupService.UpdateGroupAsync(updateDto);

                if (result.IsSuccess)
                {
                    MessageBox.Show("Grupo actualizado correctamente", "Éxito");
                    await LoadGroupsAsync();
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
        private async Task DeleteGroupAsync()
        {
            if (SelectedGroup == null)
            {
                MessageBox.Show("Seleccione un grupo primero", "Advertencia");
                return;
            }

            var confirmResult = MessageBox.Show(
                $"¿Está seguro de eliminar el grupo '{SelectedGroup.GroupName}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmResult == MessageBoxResult.Yes)
            {
                try
                {
                    var result = await _groupService.DeleteGroupAsync(SelectedGroup.GroupId);

                    if (result.IsSuccess)
                    {
                        MessageBox.Show("Grupo eliminado correctamente", "Éxito");
                        ClearForm();
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
        private void ClearForm()
        {
            SelectedGroup = null;
            GroupName = string.Empty;
            Description = string.Empty;
            IsPinned = false;
            PaginasAsignadas.Clear();
        }
    }
}