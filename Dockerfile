# ------------------------------------------------------------------------------
# DOCKERFILE - WEB XỬ LÝ ẢNH (ASP.NET Core 8 Minimal API + Frontend Vanilla)
# Single Monolithic Container: Backend API + Static Frontend (wwwroot)
# ------------------------------------------------------------------------------

# --- GIAI ĐOẠN 1: BUILD & PUBLISH ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file .csproj để cache restore layer
COPY ["backend/WebXuLyAnh.Api.csproj", "backend/"]
RUN dotnet restore "backend/WebXuLyAnh.Api.csproj"

# Copy toàn bộ mã nguồn
COPY backend/ backend/
COPY frontend/ frontend/

# Đảm bảo wwwroot luôn nhận assets mới nhất từ frontend/
RUN cp -r frontend/* backend/wwwroot/

# Build và Publish bản Release tối ưu
WORKDIR /src/backend
RUN dotnet publish "WebXuLyAnh.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- GIAI ĐOẠN 2: RUNTIME MÔI TRƯỜNG CHẠY NHẸ NHÀNG ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy thành phẩm sau khi publish
COPY --from=build /app/publish .

# Cấu hình cổng lắng nghe 5000
ENV ASPNETCORE_HTTP_PORTS=5000
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5000

ENTRYPOINT ["dotnet", "WebXuLyAnh.Api.dll"]
