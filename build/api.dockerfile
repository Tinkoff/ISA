ARG BUILD_NUMBER
FROM tinkoff-isa-build-base:$BUILD_NUMBER AS api
WORKDIR /sln

# Publish API
RUN dotnet publish Tinkoff.ISA.API -c Release -o ../published/Web --no-restore
	
# Result image
FROM microsoft/dotnet:2.2-aspnetcore-runtime AS api-final
WORKDIR /app
ENV TZ=Europe/Moscow
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
COPY --from=api ./sln/published/Web .
ENTRYPOINT ["dotnet", "Tinkoff.ISA.API.dll"]