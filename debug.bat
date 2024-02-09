@echo off
dotnet build src/Limbo.Umbraco.ModelsBuilder --configuration Debug /t:rebuild /t:pack -p:PackageOutputPath=c:\nuget\Umbraco10