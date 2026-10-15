using Avalonia.Controls;

namespace TaskTrackerAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Opened += (_, _) => NewTaskInput.Focus();
    }
}
