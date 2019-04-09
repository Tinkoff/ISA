FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /sln

# Set timezone for tests
ENV TZ=Europe/Moscow
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

COPY ./src/Tinkoff.ISA.sln ./
COPY ./src/**/*.csproj ./

# Recreation of the project folder structure by the name of csproj files, since COPY cannot transfer this structure.
RUN for f in *.csproj; do \
        filename=$(basename $f) && \
        dirname=${filename%.*} && \
        mkdir $dirname && \
        mv $filename ./$dirname/; \
    done

# NuGet restore
RUN dotnet restore Tinkoff.ISA.sln \
	-s https://api.nuget.org/v3/index.json \
	--packages /nuget/packages
    
# Copy other files
COPY ./src .

# Build
RUN dotnet build Tinkoff.ISA.sln -c Release --no-restore

# Run tests
RUN dotnet test Tinkoff.ISA.AppLayer.UnitTests/Tinkoff.ISA.AppLayer.UnitTests.csproj --no-restore && \
 	dotnet test Tinkoff.ISA.DAL.UnitTests/Tinkoff.ISA.DAL.UnitTests.csproj --no-restore && \
 	dotnet test Tinkoff.ISA.Scheduler.UnitTests/Tinkoff.ISA.Scheduler.UnitTests.csproj --no-restore