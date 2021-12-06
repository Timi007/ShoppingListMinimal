# Shopping List

University project about containerization of REST API

## Getting started

Change credentials and connection string to the database in `docker-compose.yml`
```yml
POSTGRES_USER: my_user
POSTGRES_PASSWORD: my_password
```
```yml
ConnectionStrings__ShoppingList: "Host=psqlserver;Database=ShoppingList;Username=my_user;Password=my_password"
```

After configuring run following to start all containers:
```bash
docker compose -d -p shoppinglist up
```


If API was changed after the container was setup, run following to rebuild it:
```bash
docker compose -d -p shoppinglist up --build api
```

## pgAdmin 4

The docker compose file provides an optional pgAdmin 4 profile.

Change the credentials in `docker-compose.yml`:
```yml
PGADMIN_DEFAULT_EMAIL: my_user@example.com
PGADMIN_DEFAULT_PASSWORD: my_password
```

Then start the containers:
```bash
docker compose -p shoppinglist --profile pgadmin up
```

You can now connect to the database with following values:
- Hostname: psqlserver
- Username: *Value of `POSTGRES_USER`*
- Password: *Value of `POSTGRES_PASSWORD`*

## Connection string

The connection string can be defined in two locations: `docker-compose.yml` or `ShoppingListMinimal/appsettings.json`. The YML file has priority.

`docker-compose.yml`:
```yml
ConnectionStrings__ShoppingList: "Host=psqlserver;Database=ShoppingList;Username=my_user;Password=my_password"
```

`ShoppingListMinimal/appsettings.json`:
```json
"ConnectionStrings": {
    "ShoppingList": "Host=psqlserver;Database=ShoppingList;Username=my_user;Password=my_password"
}
```

## Build docker image without docker compose

Build image where `ShoppingListMinimal.sln` is located (root project folder):
```bash
docker build -t timi007/shoppinglistapi:0.1 -t timi007/shoppinglistapi:latest -f ./ShoppingListMinimal/Dockerfile .
```

To run container without database:
```bash
docker run -d --name shoppinglistapi -p 5000:5000 -e ConnectionStrings__ShoppingList="Host=db_host;Database=ShoppingList;Username=my_user;Password=my_password" timi007/shoppinglistapi:latest
```