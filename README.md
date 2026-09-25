# amba-rename-media

Command line tool to rename media files to timebased format.

![media-rename](https://user-images.githubusercontent.com/3954761/136654239-dd30e6b5-5f4c-4f3a-830b-35c0aeb8a051.png)


## Version

The release version is `<Version>` in [Amba.RenameMedia/Amba.RenameMedia.csproj](Amba.RenameMedia/Amba.RenameMedia.csproj). Bump that value when you cut a release. `build.ps1` and Docker publish use it, and local git builds append the commit (`2.0.0+<sha>`).

```
rename-media --version
```

## Build

Use [build.ps1](build.ps1) to build .exe and dotnet tool

# Deploy to Dockerhub:

```bash
docker build -t musukvl/amba-rename-media:2.0.0 .
docker image tag musukvl/amba-rename-media:2.0.0 musukvl/amba-rename-media:latest
docker push musukvl/amba-rename-media:latest
docker push musukvl/amba-rename-media:2.0.0
```