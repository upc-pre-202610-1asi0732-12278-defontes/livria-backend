# livria-backend

## Configuración de base de datos

- **Local:** MySQL con `docker compose` en la raíz del repo; credenciales en [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) (`ConnectionStrings:DefaultConnection`, `ConnectionStrings:DbName`).
- **Azure MySQL:** ver [DEPLOY-AZURE.md](./DEPLOY-AZURE.md).