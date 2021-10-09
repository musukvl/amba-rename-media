﻿using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace Amba.RenameMedia
{
    class Program
    {
        static int Main(string[] args)
        {
            var services = new ServiceCollection()
 
                .AddSingleton<IConsole>(PhysicalConsole.Singleton)
                .BuildServiceProvider();

            var app = new CommandLineApplication<RenameMediaCommand>();
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);
            if (!args.Any())
            {
                app.ShowHelp();
                return 0;
            }
            return app.Execute(args);

        }
    }
}