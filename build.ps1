#build dotnet tool
# Version comes from <Version> in Amba.RenameMedia.csproj.
$appName = "Amba.RenameMedia"
$toolName = "amba-rename-media"
$csprojPath = "$appName/$appName.csproj"

#build nuget package
dotnet pack $csprojPath --configuration Release --output ./publish/tool `
    -p:PackAsTool=true `
    -p:ToolCommandName=$toolName

#build single file
dotnet publish $csprojPath `
    --configuration Release `
    -r win-x64 `
    --output ./publish/exe  `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=false `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugSymbols=false `
    -p:CopyOutputSymbolsToPublishDirectory=false

Move-Item -Path "./publish/exe/$appName.exe" "./publish/exe/$toolName.exe" -Force
