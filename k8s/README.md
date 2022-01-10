# Deploying with Kubernetes

## Getting Started

### Namespace

Optionally create a namespace first:
```bash
kubectl create namespace shoppinglist
```
Change to it:
```bash
kubectl config set-context --current --namespace=shoppinglist
```

### Database

It is required that an external PostgreSQL database must run.
Use this for testing purposes. Don't forget to change the credentials.
```
docker run -d -p 5432:5432 -e POSTGRES_USER=my_user -e POSTGRES_PASSWORD=my_password --name shoppinglistdatabase timi007/shoppinglistdatabase:latest
```

### Deploying API

1. First create a secret containing the connection string to the external running database. Change `Username` and `Password` to database credentials:
    ```bash
    kubectl create secret generic shoppinglistdb --from-literal=connection_string="Host=shoppinglistdatabase;Database=ShoppingList;Username=my_user;Password=my_password"
    ```

2. Setup database service so pods can access external database. **Change IP in `.yaml` to external database first!**  
    IP *must not* be loopback (127.0.0.0/8 for IPv4, ::1/128 for IPv6), or link-local (169.254.0.0/16 and 224.0.0.0/24 for IPv4, fe80::/64 for IPv6).
    ```yaml
    subsets: 
    - addresses: 
        - ip: 192.168.2.108 # Change this ip to external database
    ```
    After you can deploy the service:
    ```bash
    kubectl apply -f ./k8s_service_shoppinglistdatabase.yaml
    ```

3. Deploy API:
    ```bash
    kubectl apply -f ./k8s_deploy_shoppinglistapi.yaml
    ```

4. Expose API:
    ```bash
    kubectl apply -f ./k8s_service_external_shoppinglistapi.yaml
    ```

### Deploying API and Frontend

Follow steps 1-3 from [Deploying API](#deploying-api) first. Then continue with following:

4. Expose API internally:
    ```bash
    kubectl apply -f ./k8s_service_internal_shoppinglistapi.yaml
    ```

5. Deploy Frontend: 
    ```bash
    kubectl apply -f ./k8s_deploy_shoppinglistfrontend.yaml
    ```

6. Expose Frontend:
    ```bash
    kubectl apply -f ./k8s_service_external_shoppinglistfrontend.yaml
    ```

## Test availability

The backend API provides a kill switch at following path:
```
HTTP GET http://localhost:5000/fail
```