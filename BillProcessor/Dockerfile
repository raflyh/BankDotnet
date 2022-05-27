#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BillProcessor/BillProcessor.csproj", "BillProcessor/"]
RUN dotnet restore "BillProcessor/BillProcessor.csproj"
COPY . .
WORKDIR "/src/BillProcessor"
RUN dotnet build "BillProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BillProcessor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BillProcessor.dll"]