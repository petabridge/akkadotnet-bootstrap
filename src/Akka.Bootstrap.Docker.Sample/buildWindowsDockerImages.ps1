# Build local Windows nanoserver Docker images using the current project.
# Script is designed to be run inside the root directory of the Akka.Bootstrap.Docker.Sample project.
param (
    [string]$imageName = "akka.docker.boostrap",
    [Parameter(Mandatory=$true)][string]$tagVersion
)

Write-Host "Building project..."
dotnet publish -c Release

$linuxImage = "{0}:{1}-linux" -f $imageName,$tagVersion
$linuxImageLatest = "{0}:latest-linux" -f $imageName

Write-Host ("Creating Docker (Linux) image [{0}]..." -f $linuxImage)
docker build . -f Dockerfile-linux -t $linuxImage -t $linuxImageLatest

$windowsImage = "{0}:{1}-windows" -f $imageName,$tagVersion
$windowsImageLatest = "{0}:latest-windows" -f $imageName

Write-Host ("Creating Docker (Windows) image [{0}]..." -f $windowsImage)
docker build . -f Dockerfile-windows -t $linuxImage -t $linuxImageLatest