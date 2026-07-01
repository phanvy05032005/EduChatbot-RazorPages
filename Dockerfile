# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["EduChatbot.slnx", "./"]
COPY ["EduChatbot.Web/EduChatbot.Web.csproj", "EduChatbot.Web/"]
COPY ["EduChatbot.Business/EduChatbot.Business.csproj", "EduChatbot.Business/"]
COPY ["EduChatbot.Data/EduChatbot.Data.csproj", "EduChatbot.Data/"]
COPY ["EduChatbot.Models/EduChatbot.Models.csproj", "EduChatbot.Models/"]

# Restore NuGet packages
RUN dotnet restore "EduChatbot.slnx"

# Copy the rest of the source code
COPY . .

# Build and publish the Web project
WORKDIR "/src/EduChatbot.Web"
RUN dotnet publish "EduChatbot.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Configure ASP.NET Core to bind to the PORT environment variable set by Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EduChatbot.Web.dll"]
