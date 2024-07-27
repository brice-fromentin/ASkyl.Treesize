using System.Diagnostics;
using ASkyl.Treesize.Data;
using CommunityToolkit.Maui.Storage;

namespace ASkyl.Treesize;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private async void OnFolderClicked(object sender, EventArgs e)
	{
		var result = await FolderPicker.PickAsync("");

		var status = await Permissions.RequestAsync<Permissions.StorageRead>();

		if (status == PermissionStatus.Granted)
		{
			if (result.IsSuccessful)
			{
				Activity.IsRunning = true;
				IsEnabled = false;

				try
				{
					var watch = new Stopwatch();
					watch.Start();

					FolderNameLbl.Text = $"{result.Folder.Path} ";

					var tree = new FileSystemNode(result.Folder.Path, null);

					var progressDirectory = new Progress<string>(x => WorkingFolderLbl.Text = x);

					await Task.Run(() => ComputeSizes(tree, result.Folder.Path, progressDirectory));

					watch.Stop();

					FolderNameLbl.Text = $"Done - {tree.Size / 1024.0M / 1024.0M} MB - {watch.Elapsed.TotalSeconds}s - {FolderNameLbl.Text} ";
				}
				catch (Exception exception)
				{
					FolderNameLbl.Text = exception.Message + Environment.NewLine + exception.StackTrace;
				}

				IsEnabled = true;
				Activity.IsRunning = false;
			}
			else
			{
				FolderNameLbl.Text = "Click to select folder ...";
			}
		}
	}

	private static readonly EnumerationOptions enumOptions = new() { IgnoreInaccessible = true, RecurseSubdirectories = false };

	private static void ComputeSizes(FileSystemNode parent, string path, IProgress<string> progressDirectory)
	{
		var directory = new DirectoryInfo(path);

		ComputeFiles(parent, directory);
		ComputeDirectories(parent, directory, progressDirectory);
	}

	private static void ComputeFiles(FileSystemNode parent, DirectoryInfo directory)
	{
		try
		{
			var files = directory.EnumerateFiles("*.*", enumOptions).OrderBy(x => x.FullName);

			foreach (var file in files)
			{
				parent.AddFile(file.FullName, file.Length);
			}
		}
		catch (Exception exception)
		{
		}
	}

	private static void ComputeDirectories(FileSystemNode parent, DirectoryInfo directory, IProgress<string> progress)
	{
		progress.Report($"ComputeDirectories - {directory.FullName}");

		try
		{
			var folders = directory.EnumerateDirectories("*", enumOptions).OrderBy(x => x.FullName);

			foreach (var folder in folders)
			{
				var node = parent.AddFolder(folder.FullName);
				ComputeSizes(node, node.FullName, progress);
			}
		}
		catch (Exception exception)
		{
			progress.Report($"Exception - {directory.FullName} - {exception.Message}");
		}
	}
}

