pushd

cd App
Start-Process -FilePath "dotnet" -ArgumentList "run"

cd ..\OrderProcessingService
Start-Process -FilePath "dotnet" -ArgumentList "run"

cd ..\OrdersQueryService
Start-Process -FilePath "dotnet" -ArgumentList "run"

cd ..\ShippingService
Start-Process -FilePath "dotnet" -ArgumentList "run"

popd