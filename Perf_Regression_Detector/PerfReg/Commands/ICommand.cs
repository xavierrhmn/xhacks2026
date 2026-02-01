namespace PerfReg.Commands;

public interface ICommand
{
    Task<int> ExecuteAsync(string[] args);
    string Name { get; }
    string Description { get; }
}
