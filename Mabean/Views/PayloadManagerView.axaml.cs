using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mabean.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mabean.Views;

public partial class PayloadManagerView : UserControl
{
    public PayloadManagerView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<PayloadManagerViewModel>();
    }
}