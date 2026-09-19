FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Api/InterviewHub.Api.csproj src/Api/
COPY src/InterviewHub.Domain/InterviewHub.Domain.csproj src/InterviewHub.Domain/
RUN dotnet restore src/Api/InterviewHub.Api.csproj

COPY src/Api/ src/Api/
COPY src/InterviewHub.Domain/ src/InterviewHub.Domain/
RUN dotnet publish src/Api/InterviewHub.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "InterviewHub.Api.dll"]