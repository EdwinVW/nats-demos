Start-Process -FilePath "nats-server.exe" -ArgumentList "-p 4222 -cluster nats://localhost:5222"
Start-Process -FilePath "nats-server.exe" -ArgumentList "-p 4223 -cluster nats://localhost:5223 -routes nats://localhost:5222"
Start-Process -FilePath "nats-server.exe" -ArgumentList "-p 4224 -cluster nats://localhost:5224 -routes nats://localhost:5222"