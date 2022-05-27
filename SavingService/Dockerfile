#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SavingService/SavingService.csproj", "SavingService/"]
COPY ["BalanceService/BalanceService.csproj", "BalanceService/"]
COPY ["Database/Database.csproj", "Database/"]
COPY ["UserService/UserService.csproj", "UserService/"]
RUN dotnet restore "SavingService/SavingService.csproj"
COPY . .
WORKDIR "/src/SavingService"
RUN dotnet build "SavingService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SavingService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SavingService.dll"]