pushd

cd producer
dotnet build
Start-Process -FilePath "bin\Debug\netcoreapp3.0\producer.exe"

cd ..\consumer
dotnet build
Start-Process -FilePath "bin\Debug\netcoreapp3.0\consumer.exe"
Start-Process -FilePath "bin\Debug\netcoreapp3.0\consumer.exe"
popd