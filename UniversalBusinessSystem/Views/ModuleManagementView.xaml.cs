using System.Windows.Controls;
using UniversalBusinessSystem.ViewModels;

namespace UniversalBusinessSystem.Views;

public partial class ModuleManagementView : UserControl
{
    public ModuleManagementView()
    {
        InitializeComponent();
        // Resolve the ViewModel via DI so required services are injected
        try
        {
            DataContext = Application.Current is UniversalBusinessSystem.App app
                ? app.GetService<UniversalBusinessSystem.ViewModels.ModuleManagementViewModel>()
                : UniversalBusinessSystem.App.GetService<UniversalBusinessSystem.ViewModels.ModuleManagementViewModel>();
        }
        catch
        {
            // Fallback: do nothing, leave DataContext null to allow design-time rendering
        }
    }
}
