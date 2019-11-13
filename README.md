# NATS demos
This repo contains the code for the demos that I use during talks I give about NATS and NATS Streaming.

The slides of a talk I gave about NATS can be found [here](https://edwinvw.github.io/presentations/nats.html).

The repo contains the following demo projects:
- NATS
  - Basic messaging demos (pub/sub, request/response, queue-groups, wildcards).
- NATS-Streaming
  - BasicMessaging: basic messaging demo focussed on replaying messages.
  - Store: simple event-based distributed book-store application using NATS and NATS-Streaming.

Within every demo folder I've included some Powershell scripts to start the necessary components. 


The demos have some **prerequisites**:
1. All scripts that start NATS or NATS Streaming assume that the folders where you have installed the NATS and NATS Streaming executables are added to the `PATH` environment variable. If for some reason you don't want that, just change the path to the executable in the `start-nats.ps1`, `start-nats-streaming.ps1`, `start-local-cluster.ps1` and `add-server-to-cluster.ps1`scripts.
2. The store demo assumes there is a SQL Server running on your local machine on port 1434. If you want to use a different host or port, change the connection-string in the 3 DBContext classes in the *OrderProcesingService*, *OrdersQueryService* and *ShippingService*. Also change the host and port in the `reset.ps1` script.
3. The store demo assumes there's a folder `D:\Temp\NATSDemos\Store`. It uses this as the file-location for the file-system based storage provider for NATS Streaming. If you want to use a different folder, create it and change the folder in the `start-nats.ps1` script and the `reset.ps1` script. 
4. If you want to use [NATS Board](https://github.com/devfacet/natsboard) for monitoring, you can use the `start-monitor.ps1` script in the Store demo folder. This script assumes that have installed NATS Board (and node/npm) by executing `npm install natsboard -g`.

>Disclaimer: the code is **NOT production grade** and only for demo purposes and for experimenting with NATS and NATS Streaming.
