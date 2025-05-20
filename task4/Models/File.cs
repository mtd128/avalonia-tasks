namespace task2.Models;

public class File(string name, long size, Directory? parent = null) : FileSystemNode(name, parent)
{
    public override NodeType Type => NodeType.File;
    
    public override long Size { get; } = size;
}