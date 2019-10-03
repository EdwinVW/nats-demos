Start-Process -FilePath "nats-server.exe"
Start-Process -FilePath "nats-streaming-server.exe" -ArgumentList "--port 4223 --store FILE --dir d:\temp\NATSDemos\Store -m 8223"
