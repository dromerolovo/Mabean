using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Mabean.Views;

public partial class BehaviorChainView : UserControl
{
    public BehaviorChainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
