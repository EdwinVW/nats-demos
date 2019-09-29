pushd

cd App
dotnet build
Start-Process "bin\Debug\netcoreapp3.0\App.exe"

cd ..\OrderProcessingService
dotnet build
Start-Process "bin\Debug\netcoreapp3.0\OrderProcessingService.exe"

cd ..\OrdersQueryService
dotnet build
Start-Process "bin\Debug\netcoreapp3.0\OrdersQueryService.exe"

popd
