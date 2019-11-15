# make sure natsboard is installed:
# npm install natsboard -g

Start-Process -FilePath "natsboard" -ArgumentList "--nats-mon-url http://localhost:8223"
Start-Process -FilePath "http://localhost:8223"
Start-Process -FilePath "http://localhost:8223/streaming"
Start-Process -FilePath "http://localhost:3000"