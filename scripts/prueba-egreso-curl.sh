#!/bin/bash
# Prueba con curl: al agregar un egreso, se resta de los ingresos (balance).
# Requisitos: API corriendo en http://localhost:5229 y usuario admin/admin.
# Uso: ./scripts/prueba-egreso-curl.sh

set -e
BASE="http://localhost:5229"

echo "=== 1) Login ==="
RESP=$(curl -s -X POST "$BASE/api/auth/login" -H "Content-Type: application/json" -d '{"nombreUsuario":"admin","contrasena":"admin"}')
TOKEN=$(echo "$RESP" | python3 -c "import sys,json; print(json.load(sys.stdin).get('token',''))")
if [ -z "$TOKEN" ]; then
  echo "Error: no se obtuvo token. ¿API corriendo y usuario admin existe?"
  exit 1
fi

echo "=== 2) Dashboard ANTES de egreso ==="
curl -s -H "Authorization: Bearer $TOKEN" "$BASE/api/dashboard/summary" | python3 -m json.tool

echo ""
echo "=== 3) Crear egreso de 500 ==="
curl -s -X POST "$BASE/api/expenses" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"concept":"Prueba egreso","amount":500,"date":"2025-02-26","category":"Operativo"}' | python3 -m json.tool

echo ""
echo "=== 4) Dashboard DESPUÉS de egreso (el egreso debe restar del balance) ==="
curl -s -H "Authorization: Bearer $TOKEN" "$BASE/api/dashboard/summary" | python3 -m json.tool

echo ""
echo ">>> Si totalExpenses=500 y balance=-500, el egreso se está restando correctamente de los ingresos."
