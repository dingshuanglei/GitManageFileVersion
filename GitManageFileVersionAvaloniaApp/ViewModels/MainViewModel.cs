using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitManageFileVersionAvaloniaApp.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GitManageFileVersionAvaloniaApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    #region ObservableProperty
    /// <summary>
    /// SelectedIndex
    /// </summary>
    [ObservableProperty]
    private int selectedIndex;
    /// <summary>
    /// 关于
    /// </summary>
    [ObservableProperty]
    private string about;
    /// <summary>
    /// 路径
    /// </summary>
    [ObservableProperty]
    private string gitPath;


    #endregion

    #region Command
    /// <summary>
    /// init
    /// </summary>
    public IAsyncRelayCommand TestCommand { get; }
    /// <summary>
    /// init
    /// </summary>
    public IAsyncRelayCommand GitInitCommand { get; }

    /// <summary>
    /// open file
    /// </summary>
    public IAsyncRelayCommand OpenFileCommand { get; }

    /// <summary>
    /// open folder
    /// </summary>
    public IAsyncRelayCommand OpenFolderCommand { get; }


    #endregion



    public MainViewModel()
    {
        SelectedIndex = 1;
        About = "我是一款开源免费的Git管理文件版本";
        GitPath = AppDomain.CurrentDomain.BaseDirectory;
        GitInitCommand = new AsyncRelayCommand(GitInit);
        OpenFileCommand = new AsyncRelayCommand(OpenFile);
        OpenFolderCommand = new AsyncRelayCommand(OpenFolder);
        TestCommand = new AsyncRelayCommand(Test);
    }

    private async Task Test()
    {
        await Task.Delay(1000);
        if (SelectedIndex > 2)
        {
            SelectedIndex = 0;
        }
        else
        {
            SelectedIndex++;
        }
    }

    private async Task GitInit()
    {
        await Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(GitPath))
            {
                return ;
            }
            var infos = ExcuteGitCommand("init", GitPath);
            string errInfos = string.Empty;
            if (!string.IsNullOrWhiteSpace(errInfos))
            {

            }
        });

    }

    /// <summary>
    /// git command
    /// </summary>
    /// <param name="commnad">command</param>
    /// <param name="gitPath">git path</param>
    /// <returns></returns>
    private static List<GitInfo> ExcuteGitCommand(string commnad, string gitPath)
    {
        List<GitInfo> result = [];
        // 启动进程
        using Process process = new();
        var paths = gitPath.Split(Environment.NewLine);
        foreach (var path in paths)
        {
            process.StartInfo.FileName = "git"; // 或者 "git.exe" 如果git已添加到环境变量中
            process.StartInfo.Arguments = commnad;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WorkingDirectory = path;

            // 启动进程
            process.Start();

            // 输出命令执行结果
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            result.Add(new GitInfo(path,stdout,stderr));
        }
        process.WaitForExit();
        return result;
    }

    /// <summary>
    /// open file
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task OpenFile(CancellationToken token)
    {
        ErrorMessages?.Clear();
        try
        {
            await DoOpenFilePickerAsync();
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
    }

    /// <summary>
    /// open file picker
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    private async Task DoOpenFilePickerAsync()
    {
        // For learning purposes, we opted to directly get the reference
        // for StorageProvider APIs here inside the ViewModel. 

        // For your real-world apps, you should follow the MVVM principles
        // by making service classes and locating them with DI/IoC.

        // See IoCFileOps project for an example of how to accomplish this.
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open Text File",
            AllowMultiple = true
        });
        GitPath = string.Join(Environment.NewLine, files.Select(f => f.Path));
        GitPath = GitPath.Replace("file:///", "");
    }

    /// <summary>
    /// open folder
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task OpenFolder(CancellationToken token)
    {
        //await MessageBox.ShowAsync("123","test",MessageBoxIcon.Info,MessageBoxButton.OkCancel);
        await DoOpenFolderPickerAsync();


    }

    /// <summary>
    /// open folder picker
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    private async Task DoOpenFolderPickerAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
          desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

        var folders = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Open Folder",
            AllowMultiple = true
        });
        GitPath = string.Join(Environment.NewLine, folders.Select(f => f.Path));
        GitPath = GitPath.Replace("file:///", "");
    }

    #region git

    /// <summary>
    /// 获取环境git.ext的环境变量路径
    /// </summary>
    private static string StrEnvironmentVariable
    {
        get
        {
            string strPath = Environment.GetEnvironmentVariable("Path") ?? string.Empty;
            if (string.IsNullOrEmpty(strPath))
            {
                Debug.WriteLine(">>>>>strEnvironmentVariable: enviromentVariable is not config!!!!");
            }

            string[] strResults = strPath.Split(';');
            for (int i = 0; i < strResults.Length; i++)
            {
                if (!strResults[i].Contains(@"Git\cmd"))
                    continue;

                strPath = strResults[i];
                break;
            }

            return strPath;
        }
    }

    public static string StrWorkingDir { get; set; }


    /// <summary>
    /// 执行git指令
    /// </summary>
    public static void ExcuteGitCommand(string strCommnad, string gitPath, DataReceivedEventHandler call)
    {
        string strGitPath = System.IO.Path.Combine(StrEnvironmentVariable, "git.exe");
        if (string.IsNullOrEmpty(strGitPath))
        {
            Debug.WriteLine(">>>>>strEnvironmentVariable: enviromentVariable is not config!!!!");
            return;
        }

        Process p = new();
        p.StartInfo.FileName = strGitPath;
        //p.StartInfo.FileName = "git";
        p.StartInfo.Arguments = strCommnad;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        //p.StartInfo.WorkingDirectory = strWorkingDir;
        p.StartInfo.WorkingDirectory = gitPath;

        //p.OutputDataReceived += call;
        //p.OutputDataReceived -= OnOutputDataReceived;
        //p.OutputDataReceived += OnOutputDataReceived;

        p.Start();
        //p.BeginOutputReadLine();
        // 输出命令执行结果
        string stdout = p.StandardOutput.ReadToEnd();
        //string stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();
    }

    /// <summary>
    /// 输出git指令执行结果
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (null == e || string.IsNullOrEmpty(e.Data))
        {
            Debug.WriteLine(">>>>>>Git command error!!!!!");
            return;
        }

        Debug.WriteLine(e.Data);
    }


    #endregion

}
