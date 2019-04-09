ARG BUILD_NUMBER
FROM tinkoff-isa-build-base:$BUILD_NUMBER AS scheduler
WORKDIR /sln

# Publish Scheduler
RUN dotnet publish Tinkoff.ISA.Scheduler -c Release -o ../published/Scheduler --no-restore

# Result image
FROM microsoft/dotnet:2.2-runtime AS scheduler-final
WORKDIR /app
ENV TZ=Europe/Moscow
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
COPY --from=scheduler ./sln/published/Scheduler .
ENTRYPOINT ["dotnet", "Tinkoff.ISA.Scheduler.dll"]