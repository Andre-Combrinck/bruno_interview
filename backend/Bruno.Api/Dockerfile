FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Bruno.Domain/Bruno.Domain.csproj Bruno.Domain/
COPY Bruno.Application/Bruno.Application.csproj Bruno.Application/
COPY Bruno.Infrastructure/Bruno.Infrastructure.csproj Bruno.Infrastructure/
COPY Bruno.Api/Bruno.Api.csproj Bruno.Api/
RUN dotnet restore Bruno.Api/Bruno.Api.csproj
COPY . .
RUN dotnet publish Bruno.Api/Bruno.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Bruno.Api.dll"]
