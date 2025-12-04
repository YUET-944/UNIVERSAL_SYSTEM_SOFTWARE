using System.Windows.Controls;
using UniversalBusinessSystem.ViewModels;

namespace UniversalBusinessSystem.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        DataContext = App.GetService<DashboardViewModel>();
    }
}
