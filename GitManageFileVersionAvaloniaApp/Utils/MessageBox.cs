using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System.Threading.Tasks;

namespace GitManageFileVersionAvaloniaApp.Utils
{
    public static class MessageBox
    {
        public static async Task<MessageBoxResult> ShowAsync(string message, string title = "", MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxButton button = MessageBoxButton.OkCancel)
        {
            var messageWindow = new MessageBoxWindow(button)
            {
                Content = message,
                Title = title,
                MessageIcon = icon,
            };
            var lifetime = Application.Current?.ApplicationLifetime;
            if (lifetime is IClassicDesktopStyleApplicationLifetime classLifetime)
            {
                var main = classLifetime.MainWindow;
                if (main is null)
                {
                    messageWindow.Show();
                    return MessageBoxResult.None;
                }
                else
                {
                    var result = await messageWindow.ShowDialog<MessageBoxResult>(main);
                    return result;
                }
            }
            else
            {
                return MessageBoxResult.None;
            }

        }

    }
}
