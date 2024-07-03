using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace GitManageFileVersionAvaloniaApp.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    protected ViewModelBase()
    {
        ErrorMessages = [];
    }

    [ObservableProperty]
    private ObservableCollection<string>? _errorMessages;

}
