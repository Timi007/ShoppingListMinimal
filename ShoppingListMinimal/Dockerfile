#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://*:5000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ShoppingListMinimal/ShoppingListMinimal.csproj", "ShoppingListMinimal/"]
RUN dotnet restore "ShoppingListMinimal/ShoppingListMinimal.csproj"
COPY . .
WORKDIR "/src/ShoppingListMinimal"
RUN dotnet build "ShoppingListMinimal.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShoppingListMinimal.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShoppingListMinimal.dll"]