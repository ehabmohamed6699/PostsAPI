FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY PostsAPI/PostsAPI.csproj PostsAPI/
RUN dotnet restore PostsAPI/PostsAPI.csproj
COPY PostsAPI/ PostsAPI/
RUN dotnet publish PostsAPI/PostsAPI.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PostsAPI.dll"]