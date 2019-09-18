Start-Process -FilePath "D:\tools\nats\nats-server.exe"
Start-Process -FilePath "D:\Tools\nats-streaming\nats-streaming-server.exe" -ArgumentList "--port 4223 --store FILE --dir d:\temp\NATSDemo\StoreDemo"
