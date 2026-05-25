FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Create a non-root user to run the app
RUN adduser --disabled-password --gecos "" appuser || useradd -m appuser || true

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AssistanceManagementSystem.csproj", "./"]
RUN dotnet restore "AssistanceManagementSystem.csproj"
COPY . .
RUN dotnet publish "AssistanceManagementSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
RUN mkdir -p /app/wwwroot/uploads \
	&& mkdir -p /app/logs \
	&& chown -R appuser:appuser /app/wwwroot/uploads /app/logs || true
USER appuser
ENTRYPOINT ["dotnet", "AssistanceManagementSystem.dll"]
