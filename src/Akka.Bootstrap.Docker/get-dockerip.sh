#!/bin/sh
host=$(hostname -i)
echo "Docker container bound on $host"
export CLUSTER_IP="$host"

exec "$@"