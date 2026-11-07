#!/usr/bin/env bash
# Instala EventCR_BoleteriaPro_DB desde cero ejecutando los scripts en el orden de orden_ejecucion.txt.
# Uso:  ./instalar.sh -S localhost,1433 -U sa -P 'SuClave'   (autenticación SQL, p. ej. Docker)
#       ./instalar.sh -S localhost -E                        (autenticación integrada)
#       ./instalar.sh ... --pruebas                          (además ejecuta las pruebas de cada módulo)
# Requiere sqlcmd (go-sqlcmd: brew install sqlcmd | winget install sqlcmd).
set -euo pipefail
cd "$(dirname "$0")"
PRUEBAS=0; ARGS=()
for a in "$@"; do if [ "$a" = "--pruebas" ]; then PRUEBAS=1; else ARGS+=("$a"); fi; done
while read -r f; do
  [ -z "$f" ] && continue
  echo ">> $f"; sqlcmd "${ARGS[@]}" -C -b -i "$f"
done < orden_ejecucion.txt
echo "Base de datos EventCR_BoleteriaPro_DB instalada."
if [ "$PRUEBAS" = 1 ]; then
  for f in modulo1_usuarios/M1_03_pruebas.sql modulo2_eventos/M2_06_pruebas.sql modulo3_pedidos/M3_04_pruebas.sql \
           modulo4_pagos_qr/M4_05_pruebas.sql validacion/99_validacion_general.sql; do
    echo ">> $f"; sqlcmd "${ARGS[@]}" -C -b -W -i "$f"
  done
fi
