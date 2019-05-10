#!/usr/bin/env bash
##########################################################################
# Build local Alpine Linux Docker images using the current project.
# Script is designed to be run inside the root directory of the Akka.Bootstrap.Docker.Sample project.
##########################################################################

IMAGE_VERSION=$1
IMAGE_NAME=$2

if [-z $IMAGE_VERSION]; then
	echo `date`" - Missing mandatory argument: Docker image version."
	echo `date`" - Usage: ./buildDockerImagesLinux.sh [imageVersion] [imageName]"
fi

if [-z $IMAGE_NAME ]; then
	IMAGE_NAME = "akka.docker.boostrap"
	echo "Using default Docker image name [$IMAGE_NAME]"
fi

echo "Building project..."
exec dotnet publish -c Release

