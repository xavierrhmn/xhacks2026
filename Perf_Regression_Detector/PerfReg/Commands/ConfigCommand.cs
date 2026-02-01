using PerfReg.Configuration;

namespace PerfReg.Commands;

public class ConfigCommand : ICommand
{
    public string Name => "config";
    public string Description => "Create default configuration file";

    public Task<int> ExecuteAsync(string[] args)
    {
        ConfigLoader.SaveDefault();
        return Task.FromResult(0);
    }
}
