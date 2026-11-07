# Regenera EventCR_BoleteriaPro_DB_completo.sql (un solo archivo) a partir de orden_ejecucion.txt.
Set-Location $PSScriptRoot
$salida = @("/* EventCR_BoleteriaPro_DB — instalación completa en un solo archivo (generado desde orden_ejecucion.txt).",
            "   No editar a mano: cada módulo mantiene sus scripts en su carpeta. Regenerar tras cambiar un script. */")
foreach ($f in Get-Content .\orden_ejecucion.txt | Where-Object { $_.Trim() }) {
    $salida += ""; $salida += "/* >>>>>>>>>>>>>>>>>>>> $f <<<<<<<<<<<<<<<<<<<< */"
    $salida += Get-Content -Encoding UTF8 $f
}
$salida | Set-Content -Encoding UTF8 .\EventCR_BoleteriaPro_DB_completo.sql
Write-Host "EventCR_BoleteriaPro_DB_completo.sql regenerado."
