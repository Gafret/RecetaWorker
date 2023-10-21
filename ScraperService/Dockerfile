FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["RecipeFetcherService/RecipeFetcherService.csproj", "RecipeFetcherService/"]
RUN dotnet restore "RecipeFetcherService/RecipeFetcherService.csproj"
COPY . .
WORKDIR "/src/RecipeFetcherService"
RUN dotnet build "RecipeFetcherService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RecipeFetcherService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RecipeFetcherService.dll"]
