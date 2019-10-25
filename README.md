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

Deploy SQL Server and DB (update firewall as needed)
```
kubectl apply -f manifests/azure_v1_sqlserver.yaml
kubectl apply -f manifests/azure_v1_sqldatabase.yaml
kubectl apply -f manifests/azure_v1_sqlfirewall.yaml
```

Verify Secrets were created and Server is in Ready state

```
kubectl get secret sqlserver-query
kubectl describe azuresqlserver sqlserver-query
```

Secret will have fully qualified servername, username and password

```
$ kubectl get secret sqlserver-query -o yaml
apiVersion: v1
data:
  azuresqlservername: c3Fsc2VydmVyLXF1ZXJ5
  fullyqualifiedservername: c3Fsc2VydmVyLXF1ZXJ5LmRhdGFiYXNlLndpbmRvd3MubmV0
  fullyqualifiedusername: MTczdjhpaG1Ac3Fsc2VydmVyLXF1ZXJ5
  password: NEk6TS1+Rk8vNTF8dThvNw==
  username: MTczdjhpaG0=
kind: Secret
metadata:
  creationTimestamp: "2019-10-25T01:24:31Z"
  name: sqlserver-query
```


Deploy the app
```
kubectl apply -f manifests/k8-manifest.yaml
```

application is binding secrets to `"/etc/sqlservice"` path , to verify run cat command on the pod
```
$ k exec -it -n default cloudquery-web-xxxxxxx -- ls /etc/sqlservice
azuresqlservername        fullyqualifiedusername  username
fullyqualifiedservername  password
```

# Test the application

Navigate to LoadBalancer IP  to get to the application
```
kubectl get svc cloudquery-service
```

Click New and type database name prepended by `#`, application will read servername, user and password from mapped file paths when encountering `#` at the start of db name.

![docs](./cloudquery-add.png)

use simple query like `select 1` to verify connection