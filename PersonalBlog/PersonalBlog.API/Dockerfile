FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PersonalBlog.API/PersonalBlog.API.csproj", "PersonalBlog.API/"]
COPY ["PersonalBlog.Application/PersonalBlog.Application.csproj", "PersonalBlog.Application/"]
COPY ["PersonalBlog.Domain/PersonalBlog.Domain.csproj", "PersonalBlog.Domain/"]
COPY ["SharedKernel/SharedKernel.csproj", "SharedKernel/"]
COPY ["PersonalBlog.Infrastructure/PersonalBlog.Infrastructure.csproj", "PersonalBlog.Infrastructure/"]
RUN dotnet restore "PersonalBlog.API/PersonalBlog.API.csproj"
COPY . .
WORKDIR "/src/PersonalBlog.API"
RUN dotnet build "./PersonalBlog.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PersonalBlog.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PersonalBlog.API.dll"]
