#!/usr/bin/env bash
# Regenera EventCR_BoleteriaPro_DB_completo.sql (un solo archivo) a partir de orden_ejecucion.txt.
set -euo pipefail
cd "$(dirname "$0")"
{
  printf '/* EventCR_BoleteriaPro_DB — instalación completa en un solo archivo (generado desde orden_ejecucion.txt).\n   No editar a mano: cada módulo mantiene sus scripts en su carpeta. Regenerar tras cambiar un script. */\n'
  while read -r f; do
    [ -z "$f" ] && continue
    printf '\n/* >>>>>>>>>>>>>>>>>>>> %s <<<<<<<<<<<<<<<<<<<< */\n' "$f"
    cat "$f"; printf '\n'
  done < orden_ejecucion.txt
} > EventCR_BoleteriaPro_DB_completo.sql
echo "EventCR_BoleteriaPro_DB_completo.sql regenerado."
