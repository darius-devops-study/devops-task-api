FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY DevOpsTask/TaskApi/TaskApi.csproj DevOpsTask/TaskApi/

RUN dotnet restore DevOpsTask/TaskApi/TaskApi.csproj

COPY DevOpsTask/TaskApi/ DevOpsTask/TaskApi/

RUN dotnet publish DevOpsTask/TaskApi/TaskApi.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

USER app

ENTRYPOINT ["dotnet", "TaskApi.dll"]