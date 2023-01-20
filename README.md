# SimpleB2CEcommerceApp Backend

## Default Configuration for servers

|Server Name   |Ports   |
|---|---|
|FrontendServer   |HTTPS: 3000 <br> HTTP: 3001 |
|OAuthServer   |HTTPS: 4000 <br> HTTP: 3001   |
|ApiGatewayServer   |HTTPS: 5000 <br> HTTP: 5001   |
|CartServer   |HTTPS: 5010 <br> HTTP: 5011   |
|ProductServer   |HTTPS: 5020 <br> HTTP: 5021   |

## Tech Stacks

1. C#
2. SQL
3. ASP.NET Core Web API

## How to make a database migration and update database?
We will be using OAuthServer as an example. OAuthServer contains 2 `DbContext` which is `OAuthContext` and `KeysContext`.

For OAuthContext:
```bash
1. dotnet ef migrations add OAuth_Migrations_01 -c OAuthContext -o Infrastructure/Migrations/OAuth
2. dotnet ef database update -c OAuthContext
```

For KeysContext:
```bash
1. dotnet ef migrations add Keys_Migrations_01 -c OAuthContext -o Infrastructure/Migrations/Keys
2. dotnet ef database update -c KeysContext
```

-c: DbContext

-o: File output path

## How to create a pull request?

```bash
1. git add [File/Files]
2. git commit -m "commit message"
3. git checkout -b your-branch (For newly create) or git checkout your-branch
4. git push origin your-branch
5. Open PR from your-branch in the GitHub website.
```
