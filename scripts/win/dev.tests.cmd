pushd ..\src

dotnet restore Tinkoff.ISA.sln
dotnet test Tinkoff.ISA.AppLayer.UnitTests/Tinkoff.ISA.AppLayer.UnitTests.csproj
dotnet test Tinkoff.ISA.DAL.UnitTests/Tinkoff.ISA.DAL.UnitTests.csproj
dotnet test Tinkoff.ISA.Scheduler.UnitTests/Tinkoff.ISA.Scheduler.UnitTests.csproj

popd