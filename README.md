# amba-rename-media

Command line tool to rename media files to timebased format.

![media-rename](https://user-images.githubusercontent.com/3954761/136654239-dd30e6b5-5f4c-4f3a-830b-35c0aeb8a051.png)


## Build

Use [build.ps1](build.ps1) to build .exe and dotnet tool

# Deploy to Dockerhub:

```bash
docker build -t musukvl/amba-rename-media:1.0.2 .
docker image tag musukvl/amba-rename-media:1.0.2 musukvl/amba-rename-media:latest
docker push musukvl/amba-rename-media:latest
docker push musukvl/amba-rename-media:1.0.2
```