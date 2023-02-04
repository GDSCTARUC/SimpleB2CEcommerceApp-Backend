# SimpleB2CEcommerceApp Backend

## Default Configuration for Servers

|Server Name   |Ports   |Database Provider  |DbContext  |
|---|---|---|---|
|AuthServer   |HTTPS: 4000 <br> HTTP: 4001   |MySql <br> SqlServer  |AuthMySqlContext <br> AuthAzureSqlContext  |
|ApiGatewayServer   |HTTPS: 5000 <br> HTTP: 5001   |NaN  |NaN  |
|CartServer   |HTTPS: 5010 <br> HTTP: 5011   |MySql <br> SqlServer  |CartMySqlContext <br> CartAzureSqlContext  |
|ProductServer   |HTTPS: 5020 <br> HTTP: 5021   |MySql <br> SqlServer  |ProductMySqlContext <br> ProductAzureSqlContext  |

## Tech Stacks

1. C#
2. SQL
3. ASP.NET Core Web API
4. OAuth 2.0
5. OpenID

## Database Migration and Update

Database Providers: SqlServer and MySql.

Notes: For SqlServer in this demo, it's called AzureSql because it only use to connect to Azure Sql Server. During database migrations and update, user should be in the respective directory.

For MySql:
```bash
1. dotnet ef migrations add MySql_01 -c AuthMySqlContext -o Infrastructure/Migrations/MySql
2. dotnet ef database update -c AuthMySqlContext
```

For SqlServer (AzureSql): 
```bash
1. dotnet ef migrations add AzureSql_01 -c AuthAzureSqlContext -o Infrastructure/Migrations/AzureSql
2. dotnet ef database update -c AuthAzureSqlContext
```
