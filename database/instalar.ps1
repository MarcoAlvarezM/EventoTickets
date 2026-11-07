# Instala EventCR_BoleteriaPro_DB desde cero en el orden de orden_ejecucion.txt.
# Uso:  .\instalar.ps1 -Servidor localhost                         (autenticación de Windows)
#       .\instalar.ps1 -Servidor "localhost,1433" -Usuario sa -Clave "SuClave"
#       .\instalar.ps1 -Servidor localhost -Pruebas                (además ejecuta las pruebas por módulo)
param([string]$Servidor = "localhost", [string]$Usuario, [string]$Clave, [switch]$Pruebas)
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
$auth = if ($Usuario) { @("-U", $Usuario, "-P", $Clave) } else { @("-E") }
foreach ($f in Get-Content .\orden_ejecucion.txt | Where-Object { $_.Trim() }) {
    Write-Host ">> $f"
    sqlcmd -S $Servidor @auth -C -b -i $f
    if ($LASTEXITCODE -ne 0) { throw "Falló $f" }
}
Write-Host "Base de datos EventCR_BoleteriaPro_DB instalada."
if ($Pruebas) {
    foreach ($f in "modulo1_usuarios\M1_03_pruebas.sql","modulo2_eventos\M2_06_pruebas.sql","modulo3_pedidos\M3_04_pruebas.sql","modulo4_pagos_qr\M4_05_pruebas.sql","validacion\99_validacion_general.sql") {
        Write-Host ">> $f"; sqlcmd -S $Servidor @auth -C -b -W -i $f
    }
}
