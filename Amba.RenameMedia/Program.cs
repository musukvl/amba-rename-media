using Amba.RenameMedia;
using Spectre.Console.Cli;

var app = new CommandApp<RenameMediaCommand>();
app.Configure(config =>
{
    config.SetApplicationName("rename-media");
});
return app.Run(args);
