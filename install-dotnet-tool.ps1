$appName = "Amba.RenameMedia"
dotnet tool uninstall -g $appName
dotnet tool install $appName --global --add-source ./publish/tool