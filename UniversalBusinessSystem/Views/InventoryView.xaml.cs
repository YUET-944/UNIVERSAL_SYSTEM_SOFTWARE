using System.Windows.Controls;
using UniversalBusinessSystem.ViewModels;

namespace UniversalBusinessSystem.Views;

public partial class InventoryView : UserControl
{
    public InventoryView()
    {
        InitializeComponent();
        DataContext = App.GetService<InventoryViewModel>();
    }
}
