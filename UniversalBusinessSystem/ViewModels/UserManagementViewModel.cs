using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using UniversalBusinessSystem.Core.Entities;
using UniversalBusinessSystem.Data;

namespace UniversalBusinessSystem.ViewModels;

public partial class UserManagementViewModel : ObservableObject
{
    private readonly UniversalBusinessSystemDbContext _context;
    private Guid _organizationId;

    [ObservableProperty]
    private List<User> _users = new();

    [ObservableProperty]
    private List<Role> _roles = new();

    [ObservableProperty]
    private User? _selectedUser;

    public UserManagementViewModel(UniversalBusinessSystemDbContext context)
    {
        _context = context;
        _organizationId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Default org for demo
        
        LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            await LoadUsersAsync();
            await LoadRolesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load user data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadUsersAsync()
    {
        Users = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.OrganizationId == _organizationId)
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    private async Task LoadRolesAsync()
    {
        Roles = await _context.Roles
            .Where(r => r.OrganizationId == _organizationId)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    [RelayCommand]
    private void AddUser()
    {
        MessageBox.Show("Add User functionality would open a user creation dialog", "Add User", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void EditUser(User? user)
    {
        if (user == null) return;

        MessageBox.Show($"Edit user: {user.FullName}", "Edit User", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task DeleteUser(User? user)
    {
        if (user == null) return;

        var result = MessageBox.Show($"Are you sure you want to delete '{user.FullName}'?", 
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                await LoadUsersAsync();
                MessageBox.Show("User deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
