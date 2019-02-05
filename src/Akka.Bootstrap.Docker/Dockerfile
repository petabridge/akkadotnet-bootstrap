# Dockerfile template created and maintained by Petabridge, LLC
# See the source here: https://github.com/petabridge/akkadotnet-bootstrap

FROM microsoft/dotnet:2.0-sdk AS build-env
WORKDIR /app

# should be a comma-delimited list
ENV CLUSTER_SEEDS "[]"
ENV CLUSTER_IP ""
ENV CLUSTER_PORT "5011"

COPY . ./
RUN dotnet restore

COPY get-dockerip.sh ./get-dockerip.sh
RUN dotnet publish [PATH_TO_YOUR_.CSPROJ] -c Release --framework netcoreapp2.0 -o out
RUN ls

FROM microsoft/dotnet:2.0-runtime AS runtime
WORKDIR /app
RUN ls
COPY --from=build-env /app/[PATH_TO_YOUR_.CSPROJ]/out ./

# Akka.Cluster endpoint
EXPOSE 5011

CMD ["dotnet", "[YOUR .CSPROJ .DLL OUTPUT]"]