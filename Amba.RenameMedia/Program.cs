using Amba.RenameMedia;
using Spectre.Console.Cli;

var app = new CommandApp<RenameMediaCommand>();
app.Configure(config =>
{
    config.SetApplicationName("rename-media");
    config.SetApplicationVersion(AppVersion.Current);
});
return app.Run(args);
