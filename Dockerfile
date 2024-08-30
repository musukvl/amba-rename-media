# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY Amba.RenameMedia/*.csproj ./Amba.RenameMedia/
WORKDIR /app/Amba.RenameMedia

RUN dotnet restore
COPY Amba.RenameMedia/. ./

RUN dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained false -o /app/publish

# Use the official .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
WORKDIR /media
ENTRYPOINT ["/app/Amba.RenameMedia"]