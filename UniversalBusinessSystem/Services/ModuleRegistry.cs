using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using UniversalBusinessSystem.Views;

namespace UniversalBusinessSystem.Services;

public interface IModuleRegistry
{
    IReadOnlyDictionary<string, ModuleDescriptor> Descriptors { get; }
    ModuleDescriptor? GetDescriptor(string key);
}

public class ModuleDescriptor
{
    public ModuleDescriptor(string key, string name, string icon, Func<IServiceProvider, UserControl> viewFactory)
    {
        Key = key;
        Name = name;
        Icon = icon;
        ViewFactory = viewFactory;
    }

    public string Key { get; }
    public string Name { get; }
    public string Icon { get; }
    public Func<IServiceProvider, UserControl> ViewFactory { get; }
}

public class ModuleRegistry : IModuleRegistry
{
    private readonly IReadOnlyDictionary<string, ModuleDescriptor> _descriptors;

    public ModuleRegistry()
    {
        _descriptors = new Dictionary<string, ModuleDescriptor>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "user_management",
                new ModuleDescriptor(
                    "user_management",
                    "User Management",
                    "Account",
                    sp => sp.GetRequiredService<UserManagementView>())
            },
            {
                "inventory_management",
                new ModuleDescriptor(
                    "inventory_management",
                    "Inventory Management",
                    "Package",
                    sp => sp.GetRequiredService<InventoryView>())
            }
        };
    }

    public IReadOnlyDictionary<string, ModuleDescriptor> Descriptors => _descriptors;

    public ModuleDescriptor? GetDescriptor(string key)
    {
        return _descriptors.TryGetValue(key, out var descriptor) ? descriptor : null;
    }
}
