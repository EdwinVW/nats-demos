pushd

cd producer
dotnet build
Start-Process -FilePath "dotnet" -ArgumentList "bin\Debug\netcoreapp3.0\producer.dll"

cd ..\consumer
dotnet build
Start-Process -FilePath "dotnet" -ArgumentList "bin\Debug\netcoreapp3.0\consumer.dll"
Start-Process -FilePath "dotnet" -ArgumentList "bin\Debug\netcoreapp3.0\consumer.dll nats://localhost:4223"
Start-Process -FilePath "dotnet" -ArgumentList "bin\Debug\netcoreapp3.0\consumer.dll nats://localhost:4224"
popd