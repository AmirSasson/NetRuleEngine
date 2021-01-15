dotnet pack -c release
dotnet nuget push src\NetRuleEngine\bin\Release\NetRuleEngine.{VERSION}.nupkg --api-key {API KEY} --source https://api.nuget.org/v3/index.json