namespace ASkyl.Treesize.Data;

public class FileSystemNode(string path, FileSystemNode? parent)
{
    public string FullName { get; set; } = path;

    public string Name { get; set; } = Path.GetFileName(path) ?? "";

    public long Size { get; set; } = 0L;

    public FileSystemNode? Parent { get; set; } = parent;

    public List<FileSystemNode> Folders { get; set; } = [];

    public List<FileSystemNode> Files { get; set; } = [];

    private FileSystemNode AddChild(List<FileSystemNode> entries, string path)
    {
        var entry = new FileSystemNode(path, this);
        entries.Add(entry);
        return entry;
    }

    private void AddSize(long size)
    {
        this.Size += size;
        this.Parent?.AddSize(size);
    }

    public FileSystemNode AddFolder(string path)
        => this.AddChild(this.Folders, path);

    public FileSystemNode AddFile(string path, long size)
    {
        var entry = this.AddChild(this.Files, path);
        
        entry.AddSize(size);

        return entry;
    }
}
