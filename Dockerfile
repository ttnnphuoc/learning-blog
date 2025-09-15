# ========================================
# STAGE 1: BUILD STAGE (Heavy SDK Image)
# ========================================

# STEP 1: Download .NET 9 SDK image (contains compilers, build tools)
# This image is ~2GB but has everything needed to build .NET apps
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# STEP 2: Create /app directory inside container and set as working directory
# All subsequent commands will run from /app directory
WORKDIR /app

# STEP 3: Copy only project files first (for Docker layer caching optimization)
# This step copies dependency definitions but not source code
# If these files don't change, Docker will reuse cached layer for restore step
COPY *.sln ./                                                           # Copy solution file to /app/
COPY src/BlogAPI.Domain/BlogAPI.Domain.csproj ./src/BlogAPI.Domain/     # Copy Domain project file
COPY src/BlogAPI.Application/BlogAPI.Application.csproj ./src/BlogAPI.Application/ # Copy Application project file  
COPY src/BlogAPI.Infrastructure/BlogAPI.Infrastructure.csproj ./src/BlogAPI.Infrastructure/ # Copy Infrastructure project file
COPY src/BlogAPI.WebAPI/BlogAPI.WebAPI.csproj ./src/BlogAPI.WebAPI/     # Copy WebAPI project file

# STEP 4: Download and restore all NuGet packages
# This creates a cached layer - if .csproj files unchanged, this step is skipped
# Downloads packages to /root/.nuget cache inside container
RUN dotnet restore

# STEP 5: Copy all remaining source code (.cs files, appsettings.json, etc.)
# Done after restore so source code changes don't invalidate package cache
COPY . .

# STEP 6: Compile the application in Release mode
# Creates compiled .dll files in bin/Release directories
# --no-restore skips package download (already done in step 4)
RUN dotnet build -c Release --no-restore

# STEP 7: Create deployable application package
# Copies all runtime dependencies to /app/publish directory
# This includes your app DLLs, referenced packages, runtime files
RUN dotnet publish src/BlogAPI.WebAPI/BlogAPI.WebAPI.csproj -c Release -o /app/publish --no-restore

# ========================================
# STAGE 2: RUNTIME STAGE (Lightweight Image) 
# ========================================

# STEP 8: Start fresh with smaller runtime-only image
# This image is ~200MB, contains only what's needed to RUN .NET apps (no build tools)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# STEP 9: Create /app directory in the new runtime container
WORKDIR /app

# STEP 10: Create non-root user for security (prevents privilege escalation attacks)
# Creates 'dotnet' user with ID 1001, no shell access
RUN addgroup --system --gid 1001 dotnet && \
    adduser --system --uid 1001 --gid 1001 --shell /bin/false dotnet

# STEP 11: Copy ONLY the published app from build stage to runtime stage
# --from=build references the first stage
# This copies /app/publish from build container to /app in runtime container
# Source code and build tools are left behind (smaller final image)
COPY --from=build /app/publish .

# STEP 12: Give ownership of app files to dotnet user and switch to that user
# Security: App runs as non-root user instead of root
RUN chown -R dotnet:dotnet /app
USER dotnet

# STEP 13: Document that app uses port 8080 (doesn't actually publish the port)
EXPOSE 8080

# STEP 14: Configure ASP.NET Core to listen on port 8080 and set production environment
# ASPNETCORE_URLS: Tells ASP.NET which port/interface to listen on
# ASPNETCORE_ENVIRONMENT: Sets environment (affects logging, error handling, etc.)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# STEP 15: Configure health monitoring
# Docker will ping /health endpoint every 30 seconds
# If endpoint fails 3 times, container is marked as unhealthy
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# STEP 16: Define the command to run when container starts
# This executes: dotnet BlogAPI.WebAPI.dll
# Your application starts and begins listening for HTTP requests
ENTRYPOINT ["dotnet", "BlogAPI.WebAPI.dll"]