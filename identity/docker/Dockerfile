FROM mcr.microsoft.com/dotnet/sdk:6.0

ENV ASPNETCORE_URLS http://+:5005
ENV ASPNETCORE_ENVIRONMENT=Server

COPY ./Identity/ app/
WORKDIR /app

ENTRYPOINT ["dotnet", "Palantir.Identity.dll"]
