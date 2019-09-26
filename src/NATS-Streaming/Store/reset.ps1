pushd

cd OrderProcessingService
if (Test-Path WriteModel.db) { Remove-Item -Path WriteModel.db }
if (Test-Path WriteModel.db-journal) { Remove-Item -Path WriteModel.db-journal }
dotnet ef database update

cd ..\OrdersQueryService
if (Test-Path ReadModel.db) { Remove-Item -Path ReadModel.db }
if (Test-Path ReadModel.db-journal) { Remove-Item -Path ReadModel.db-journal }
dotnet ef database update

cd ..\ShippingService
if (Test-Path Shipping.db) { Remove-Item -Path Shipping.db }
if (Test-Path Shipping.db-journal) { Remove-Item -Path Shipping.db-journal }
dotnet ef database update

Get-ChildItem -Path D:\Temp\NATSDemo\StoreDemo -Include * | remove-Item -recurse

popd