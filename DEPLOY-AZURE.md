# Backend + Azure Database for MySQL

El API arma la cadena así: `ConnectionStrings:DefaultConnection` + `database=` + `ConnectionStrings:DbName` (ver `Program.cs`).

## 1. Cadena base (`DefaultConnection`)

Debe incluir **servidor**, **usuario**, **contraseña** y **`SslMode=Required`** (Azure MySQL exige TLS). Ejemplo (sustituye la contraseña; no la subas al repo):

```text
Server=livriadb.mysql.database.azure.com;Port=3306;User ID=LivriaAdmin;Password=TU_CONTRASEÑA;SslMode=Required;
```

- Termina en `;` antes de que el código añada `database=livriadb;`.
- Usuario: el admin que creaste en Flexible Server (`LivriaAdmin`).

## 2. Nombre de base (`DbName`)

En `appsettings.json` ya está `livriadb`. En Azure debes haber creado la base con ese mismo nombre (o cambia `ConnectionStrings:DbName` en configuración).

## 3. Dónde configurar (sin commitear secretos)

### Desarrollo local contra Azure

El `TargetFramework` del `.csproj` debe ser compatible con el **SDK instalado**: con **SDK 9** podés compilar **`net8.0`** o inferior; para **`net10.0`** hace falta el **SDK 10** ([descarga](https://dotnet.microsoft.com/download)).

Desde la carpeta del proyecto `LivriaBackend`:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=livriadb.mysql.database.azure.com;Port=3306;User ID=LivriaAdmin;Password=TU_CONTRASEÑA;SslMode=Required;" --project LivriaBackend.csproj
```

En PowerShell, si la contraseña tiene caracteres especiales (`#`, `$`, etc.), usa comillas simples para el valor externo o escapa según corresponda.

### Azure App Service (recomendado para este API)

#### Crear el Web App

1. Portal → **Create a resource** → **Web App**.
2. **Publish**: Code. **Runtime stack**: **.NET 8 (LTS)** — debe coincidir con el `TargetFramework` del `.csproj` (`net8.0`).
3. **Operating System**: **Linux** suele ser el estándar para .NET Core; **Windows** también vale (IIS + ANCM).
4. **Region**: misma región que MySQL si es posible (menor latencia).
5. **App Service plan**: al menos **B1** para pruebas serias (F1/Demo puede ir justo de CPU/memoria).

#### Publicar el código

- **Visual Studio**: clic derecho en el proyecto → *Publish* → Azure → App Service → perfil nuevo (el IDE suele usar RID correcto para Linux).
- **CLI** (ejemplo, ajusta nombres):

```bash
dotnet publish LivriaBackend\LivriaBackend.csproj -c Release -o .\publish
cd publish
zip -r ../app.zip .
az webapp deployment source config-zip --resource-group RG_LIVRIA --name NOMBRE_APP --src ../app.zip
```

(O `az webapp up` si generáis la app desde cero con la CLI.)

##### App Service **Linux** + ZIP desde Windows (PowerShell)

Desde la raíz del repo **`livria-backend`** (ajustá `--resource-group` y `--name` si cambian):

1. Publicá con **`-r linux-x64`** (evita `runtimes\win-x64` en el paquete).
2. Eliminá **`publish\runtimes\browser`** si existe (opcional; a veces sobra en API).
3. **El ZIP no puede llevar `\` en los nombres de las entradas.** Si Kudu muestra `es-es\LivriaBackend.resources.dll` o `Invalid argument (22)` en rsync, el paquete está mal: casi siempre viene de **`Compress-Archive`**, **Publish ZIP desde Visual Studio**, **7-Zip por defecto** o la tarea **Archive files** de Azure DevOps/GitHub Actions. **No subas ese archivo.**

**Recomendado en Windows (sin WSL):** el **`tar.exe`** del sistema suele generar rutas con **`/`** (compatible con App Service Linux):

```powershell
cd C:\Users\alext\proyectos\livria-backend

Remove-Item -Recurse -Force .\publish -ErrorAction SilentlyContinue
dotnet publish .\LivriaBackend\LivriaBackend.csproj -c Release -r linux-x64 --self-contained false -o .\publish

$browserPath = Join-Path .\publish "runtimes\browser"
if (Test-Path $browserPath) { Remove-Item -Recurse -Force $browserPath }

Remove-Item .\deploy.zip -ErrorAction SilentlyContinue
# -C publish . = contenido de publish en la raíz del zip (sin prefijo publish/)
tar.exe -caf deploy.zip -C publish .

az webapp deploy --resource-group LivriaResources --name LivriaBackend --src-path "$PWD\deploy.zip" --type zip
```

**Comprobar antes de subir** (no debe listar ninguna ruta con `\`):

```powershell
Add-Type -AssemblyName System.IO.Compression.FileSystem
$z = [System.IO.Compression.ZipFile]::OpenRead((Join-Path (Get-Location) 'deploy.zip'))
$z.Entries | Where-Object { $_.FullName -match '\\' } | Select-Object -ExpandProperty FullName
$z.Dispose()
```

Si aparece cualquier línea, **no uses** ese `deploy.zip` para `az webapp deploy`.

**Alternativa (PowerShell, sin tar):** empaquetar con **`ZipArchive`** y forzar **`/`** en cada ruta relativa:

```powershell
cd C:\Users\alext\proyectos\livria-backend

Remove-Item -Recurse -Force .\publish -ErrorAction SilentlyContinue
dotnet publish .\LivriaBackend\LivriaBackend.csproj -c Release -r linux-x64 --self-contained false -o .\publish

$browserPath = Join-Path .\publish "runtimes\browser"
if (Test-Path $browserPath) { Remove-Item -Recurse -Force $browserPath }

$root = (Resolve-Path .\publish).Path.TrimEnd('\')
$zipPath = Join-Path ((Get-Location).Path) 'deploy.zip'
Remove-Item -LiteralPath $zipPath -ErrorAction SilentlyContinue

Add-Type -AssemblyName System.IO.Compression
$zipStream = [System.IO.File]::Open($zipPath, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write)
try {
  $archive = New-Object System.IO.Compression.ZipArchive($zipStream, [System.IO.Compression.ZipArchiveMode]::Create, $false)
  try {
    Get-ChildItem -LiteralPath $root -Recurse -File | ForEach-Object {
      $rel = ($_.FullName.Substring($root.Length) -replace '^[/\\]+', '') -replace '\\', '/'
      $entry = $archive.CreateEntry($rel, [System.IO.Compression.CompressionLevel]::Optimal)
      $entryStream = $entry.Open()
      try {
        $fileStream = [System.IO.File]::OpenRead($_.FullName)
        try { $fileStream.CopyTo($entryStream) } finally { $fileStream.Dispose() }
      } finally { $entryStream.Dispose() }
    }
  } finally { $archive.Dispose() }
} finally { $zipStream.Dispose() }

az webapp deploy --resource-group LivriaResources --name LivriaBackend --src-path "$PWD\deploy.zip" --type zip
```

**Otra opción:** **WSL** o **Git Bash**: `cd publish && zip -r ../deploy.zip .`

#### Application settings (Configuration → Application settings)

| Nombre | Valor / notas |
|--------|----------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Cadena de la sección 1 (con contraseña). |
| `ConnectionStrings__DbName` | `livriadb` (por si no cargáis `appsettings` completo). |
| `TokenSettings__Secret` | Cadena **≥ 32 caracteres** (JWT). Mismo valor que uséis en apps; no dejes el placeholder del repo. |

Guardar y **Restart** el App Service.

#### MySQL firewall y salida del App Service

- En el **App Service** → **Properties** (o *Networking* / documentación) obtened las **Outbound addresses** (y si aplica **Outbound subnet IPs**).
- En **Azure Database for MySQL** → **Networking** → reglas de firewall: permitid esas IPs (o un rango si Azure lo documenta para vuestro plan).
- Alternativa menos restrictiva: **“Allow public access from Azure services within Azure”** en MySQL (solo si aceptáis el modelo de seguridad; mejor reglas por IP de salida del App Service).

#### Puerto (Kestrel)

En App Service **no** se fuerza el puerto 5119: el código detecta `WEBSITE_INSTANCE_ID` y deja el host por defecto (Linux usa la variable **`PORT`**, p. ej. 8080).

#### HTTPS y Swagger

- El tráfico público suele entrar por **HTTPS** al front door de Azure; el runtime puede ver HTTP internamente (`UseHttpsRedirection` está activo).
- Swagger está bajo `/swagger` cuando el entorno es Development o Production (`Program.cs`). Probad: `https://NOMBRE_APP.azurewebsites.net/swagger`.

#### Opcional

- **Always On**: activar si no queréis cold start tras inactividad (planes de pago).
- **Health check**: ruta ligera si la añadís más adelante.
- **Logging**: Application Insights para trazas y fallos.

### Azure Container Apps / AKS

Define las mismas claves como secretos de entorno inyectados al contenedor.

## 4. Firewall de MySQL

Permite la IP de salida del servicio que hospeda el API (o usa **Private Endpoint** + red integrada). Si la conexión falla con timeout, suele ser firewall o VNet.

### Si ves timeout al conectar desde tu PC

1. Añade tu **IPv4 pública** en las reglas de firewall del servidor MySQL.
2. Comprueba acceso público al servidor MySQL según cómo lo creaste.
3. En PowerShell: `Test-NetConnection livriadb.mysql.database.azure.com -Port 3306` → `TcpTestSucceeded` debe ser `True`.

## 5. Migraciones / esquema

El arranque usa `EnsureCreated()`. Para Azure en producción suele ser preferible **migraciones explícitas** (`dotnet ef database update` en CI/CD) y revisar si queréis sustituir `EnsureCreated` por `Migrate()` cuando estéis listos.

## 6. Seguridad

- No commitees contraseñas ni `appsettings.Production.json` con secretos.
- Si la contraseña se compartió por chat, email o ticket, **cámbiala en Azure** y actualiza secretos / App Settings.
