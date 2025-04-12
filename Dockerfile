# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install the remote debugger dependencies
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN apt-get update && apt-get install -y unzip procps

COPY ["RecipeProject/RecipeProject.csproj", "RecipeProject/"]
RUN dotnet restore "RecipeProject/RecipeProject.csproj"
COPY . .
WORKDIR "/src/RecipeProject"
RUN dotnet build "RecipeProject.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "RecipeProject.csproj" -c Release -o /app/publish

# Use the official .NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# Install the remote debugger dependencies in the runtime image as well
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN apt-get update && apt-get install -y unzip procps

WORKDIR /app
EXPOSE 80
EXPOSE 2001

# Create directories for debugger logs
RUN mkdir -p /var/log/rider-debugger
RUN mkdir -p /home/site/wwwroot/remote-debugger

# Copy the published application from the build stage
COPY --from=publish /app/publish .

# Define entry point for the application
ENTRYPOINT ["dotnet", "RecipeProject.dll"]