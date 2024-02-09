@echo off
dotnet build src/Limbo.Umbraco.ModelsBuilder --configuration Release /t:rebuild /t:pack -p:PackageOutputPath=../../releases/nuget