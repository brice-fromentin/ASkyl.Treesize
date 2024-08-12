using System.Diagnostics;
using CommunityToolkit.Maui.Storage;
using ASkyl.Treesize.Data;
using System.Text;

namespace ASkyl.Treesize;

public partial class MainPage : ContentPage
{
	private FileSystemNode? _tree;
	private TimeSpan? _elapsed;

	public MainPage()
	{
		InitializeComponent();
	}

	private async void OnFolderClicked(object sender, EventArgs e)
	{
		Activity.IsRunning = true;
		IsEnabled = false;

		var result = await FolderPicker.PickAsync("");

		if (result.IsSuccessful)
		{
			var path = result.Folder.Path;

			try
			{
				var progressDirectory = new Progress<string>(x => WorkingFolderLbl.Text = x);

				FolderNameLbl.Text = $"{path} ";
				WorkingFolderLbl.Text = "";
				
				await  BrowsePath(path, progressDirectory);

				var totalSize = (_tree?.Size ?? 0) / 1024.0M / 1024.0M;
				var elapsedTime = _elapsed?.TotalSeconds ?? 0;

				FolderNameLbl.Text = $"Done - {totalSize} MB - {elapsedTime}s - {path} ";
			}
			catch (Exception exception)
			{
				var message = new StringBuilder();
				message.AppendLine(exception.Message);
				message.AppendLine(exception.StackTrace);

				FolderNameLbl.Text = message.ToString();
			}
		}

		IsEnabled = true;
		Activity.IsRunning = false;
	}

	private async Task BrowsePath(string path, Progress<string> progressDirectory)
	{
		var watch = Stopwatch.StartNew();

		_tree = new FileSystemNode(path, null);

		await ComputeSizes(_tree, progressDirectory);

		watch.Stop();

		_elapsed = watch.Elapsed;
	}

	private static readonly EnumerationOptions enumOptions = new() { IgnoreInaccessible = true, RecurseSubdirectories = false };

	private static async Task ComputeSizes(FileSystemNode parent, IProgress<string> progressDirectory)
	{
		var directory = new DirectoryInfo(parent.FullName);

		await ComputeDirectoriesAsync(parent, directory, progressDirectory).ConfigureAwait(false);
		ComputeFiles(parent, directory);
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
		catch (Exception ex)
		{
			// Log or handle the exception as needed
			Console.WriteLine($"Exception in ComputeFiles: {ex.Message}");
		}
	}

	private static async Task ComputeDirectoriesAsync(FileSystemNode parent, DirectoryInfo directory, IProgress<string> progress)
	{
		try
		{
			var folders = directory.EnumerateDirectories("*", enumOptions).OrderBy(x => x.FullName);

			var tasks = folders.Select(async folder =>
			{
				var node = parent.AddFolder(folder.FullName);
				await Task.Run(() => ComputeSizes(node, progress));
			});

			await Task.WhenAll(tasks);
		}
		catch (Exception exception)
		{
			progress.Report($"Exception - {directory.FullName} - {exception.Message} - {exception.StackTrace}");
		}
	}
}
