cd ..\producer
Start-Process -FilePath "dotnet" -ArgumentList "run"

cd ..\consumer
Start-Process -FilePath "dotnet" -ArgumentList "run"
Start-Process -FilePath "dotnet" -ArgumentList "run"
