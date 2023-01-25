# SimpleB2CEcommerceApp Backend

## Default Configuration for servers

|Server Name   |Ports   |
|---|---|
|AuthServer   |HTTPS: 4000 <br> HTTP: 4001   |
|ApiGatewayServer   |HTTPS: 5000 <br> HTTP: 5001   |
|CartServer   |HTTPS: 5010 <br> HTTP: 5011   |
|ProductServer   |HTTPS: 5020 <br> HTTP: 5021   |

## Tech Stacks

1. C#
2. SQL
3. ASP.NET Core Web API

## How to make a database migration and update database?
For OAuthContext:
```bash
1. dotnet ef migrations add Auth_Migrations_01
2. dotnet ef database update
```

## How to create a pull request?

```bash
1. git add [File/Files]
2. git commit -m "commit message"
3. git checkout -b your-branch (For newly create) or git checkout your-branch
4. git push origin your-branch
5. Open PR from your-branch in the GitHub website.
```
