# Shopping List

University project about containerization of REST API

## Getting started

Change credentials in `docker-compose.yml`
```yml
POSTGRES_USER: my_user
POSTGRES_PASSWORD: my_password
```
And in the `ShoppingListMinimal\appsettings.json`:
```json
"ConnectionStrings": {
    "ShoppingList": "Host=psqlserver;Database=ShoppingList;Username=my_user;Password=my_password"
}
```

After configuring run following to start all containers:
```bash
docker compose -p shoppinglist up
```


If API was changed after the container was setup, run following to rebuild it:
```
docker compose -p shoppinglist up --build api
```

## pgAdmin 4

The docker compose file provides an optional pgAdmin 4 profile.

Change the credentials in `docker-compose.yml`:
```yml
PGADMIN_DEFAULT_EMAIL: my_user@example.com
PGADMIN_DEFAULT_PASSWORD: my_password
```

Then start the containers:
```
docker compose -p shoppinglist --profile pgadmin up
```

You can then connect to the database with following values:
- Hostname: psqlserver
- Username: *Value of `POSTGRES_USER`*
- Password: *Value of `POSTGRES_PASSWORD`*
