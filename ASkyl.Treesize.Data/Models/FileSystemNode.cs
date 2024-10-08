﻿namespace ASkyl.Treesize.Data;

public class FileSystemNode(string path, FileSystemNode? parent)
{
    public string FullName { get; set; } = path;

    public string Name { get; set; } = Path.GetFileName(path) ?? "";

    public long Size { get; private set; } = 0L;

    public FileSystemNode? Parent { get; } = parent;

    public List<FileSystemNode> Folders { get; } = [];

    public List<FileSystemNode> Files { get; } = [];

    public int TotalCount
    {
        get => this.Files.Count + this.Folders.Count + this.Folders.Sum(x => x.TotalCount);
    }

    private FileSystemNode AddChild(List<FileSystemNode> entries, string path)
    {
        var entry = new FileSystemNode(path, this);
        entries.Add(entry);

        return entry;
    }

    private void AddSize(long size)
    {
        lock (this)
        {
            this.Size += size;
        }

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
