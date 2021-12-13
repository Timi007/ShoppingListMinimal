# Shopping List

<p align="center">
    <a href="https://hub.docker.com/r/timi007/shoppinglistapi">
        <img alt="Docker" src="https://badgen.net/badge/icon/docker?icon=docker&label">
    </a>
    <a href="https://hub.docker.com/r/timi007/shoppinglistapi">
        <img alt="Docker Image Size" src="https://img.shields.io/docker/image-size/timi007/shoppinglistapi/latest">
    </a>
    <a href="https://hub.docker.com/r/timi007/shoppinglistapi">
        <img alt="Docker Pulls" src="https://img.shields.io/docker/pulls/timi007/shoppinglistapi">
    </a>
    <a href="https://github.com/Timi007/ShoppingListMinimal/blob/master/LICENSE">
        <img alt="GitHub" src="https://img.shields.io/github/license/Timi007/ShoppingListMinimal">
    </a>
    <img alt="Docker Image Version" src="https://img.shields.io/docker/v/timi007/shoppinglistapi/latest">
</p>

<p align="center">
    University project about containerization of REST API.
</p>

## Getting started

Change credentials of the database in `.env`:
```env
DB_USER=my_user
DB_PASSWORD=my_password
```

After configuring run following to start all containers:
```bash
docker compose -d -p shoppinglist up
```

The API will start listing on port `5000` (or whatever was set in `.env`).

## Run API without docker compose

To run container without database:
```bash
docker run -d --name shoppinglistapi -p 5000:5000 -e ConnectionStrings__ShoppingList="Host=db_host;Database=ShoppingList;Username=my_user;Password=my_password" timi007/shoppinglistapi:latest
```

## pgAdmin 4

The docker compose file provides an optional pgAdmin 4 profile.

Change the credentials in `.env`:
```env
PGADMIN_EMAIL=my_user@example.com
PGADMIN_PASSWORD=my_password
```

Then start the containers:
```bash
docker compose -d -p shoppinglist --profile pgadmin up
```

You can now open pgAdmin [http://localhost:5050/](http://localhost:5050/) and connect to the database with following values:
- Hostname: psqlserver
- Username: *Value of `DB_USER`*
- Password: *Value of `DB_PASSWORD`*

## Additional information

### Alternative location of the connection string

The connection string can also be defined in `ShoppingListMinimal/appsettings.json`. The `.env` file has priority when using `docker compose`.

`ShoppingListMinimal/appsettings.json`:
```json
"ConnectionStrings": {
    "ShoppingList": "Host=psqlserver;Database=ShoppingList;Username=my_user;Password=my_password"
}
```

### Building docker image

Build image where `ShoppingListMinimal.sln` is located (root project folder):
```bash
docker build -t timi007/shoppinglistapi:0.1 -t timi007/shoppinglistapi:latest -f ./ShoppingListMinimal/Dockerfile .
```

### Rebuild with docker compose

If API was changed after the container was setup, run following to rebuild it:
```bash
docker compose -d -p shoppinglist up --build api
```
