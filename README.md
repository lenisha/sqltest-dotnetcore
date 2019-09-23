## Build image
Dockerfile is utilizing Multi Stage builds and is installing [MSSQL ODBC Driver 17](https://docs.microsoft.com/en-us/sql/connect/odbc/linux-mac/installing-the-microsoft-odbc-driver-for-sql-server?view=sql-server-2017) for Debian9 in the image .

```
docker build -t cloudquery .
docker run -d -p 8080:80 --name cloudquery cloudquery
docker exec -it cloudquery bash

docker login <REGISTRY_NAME>.azurecr.io
docker tag cloudquery <REGISTRY_NAME>.azurecr.io/cloudquery
docker push <REGISTRY_NAME>.azurecr.io/cloudquery
```

or using ACR

```
az login
az acr build --registry <REGISTRY_NAME> --image cloudquery:latest . 
```

## Kube

```
kubectl apply -f k8-manifest.yaml
```