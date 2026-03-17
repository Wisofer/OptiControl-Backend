#!/bin/bash
# Pruebas API OptiControl: login, clientes, productos, servicios, ventas
set -e
BASE="http://localhost:5229"
echo "=== 1. Login ==="
LOGIN=$(curl -s -X POST "$BASE/api/auth/login" -H "Content-Type: application/json" -d '{"nombreUsuario":"admin","contrasena":"admin"}')
TOKEN=$(echo "$LOGIN" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
if [ -z "$TOKEN" ]; then echo "Login falló"; exit 1; fi
echo "OK - Token obtenido"

AUTH="Authorization: Bearer $TOKEN"

echo ""
echo "=== 2. Crear clientes ==="
C1=$(curl -s -X POST "$BASE/api/clients" -H "Content-Type: application/json" -H "$AUTH" -d '{"name":"María López","email":"maria@test.com","phone":"8888-1111","address":"Managua"}')
C2=$(curl -s -X POST "$BASE/api/clients" -H "Content-Type: application/json" -H "$AUTH" -d '{"name":"Carlos Ruiz","email":"carlos@test.com","phone":"8888-2222","address":"Granada"}')
C3=$(curl -s -X POST "$BASE/api/clients" -H "Content-Type: application/json" -H "$AUTH" -d '{"name":"Ana Martínez","email":"ana@test.com","phone":"8888-3333","graduacionOd":"-2.00","graduacionOi":"-1.75"}')
IDC1=$(echo "$C1" | grep -o '"id":[0-9]*' | head -1 | grep -o '[0-9]*')
IDC2=$(echo "$C2" | grep -o '"id":[0-9]*' | head -1 | grep -o '[0-9]*')
echo "Cliente 1 id=$IDC1, Cliente 2 id=$IDC2"

echo ""
echo "=== 3. Crear 10 productos (inventario) ==="
P1R=$(curl -s -X POST "$BASE/api/products" -H "Content-Type: application/json" -H "$AUTH" -d '{
  "nombreProducto": "Producto Óptico 1",
  "tipoProducto": "montura",
  "marca": "Marca1",
  "precioCompra": 60,
  "precio": 120,
  "stock": 20,
  "stockMinimo": 3,
  "descripcion": "Descripción producto 1",
  "proveedor": "Proveedor 1"
}')
PID1=$(echo "$P1R" | grep -o '"id":[0-9]*' | head -1 | grep -o '[0-9]*')
echo "  Producto 1 id=$PID1"
for i in 2 3 4 5 6 7 8 9 10; do
  curl -s -X POST "$BASE/api/products" -H "Content-Type: application/json" -H "$AUTH" -d "{
    \"nombreProducto\": \"Producto Óptico $i\",
    \"tipoProducto\": \"montura\",
    \"marca\": \"Marca$i\",
    \"precioCompra\": $((50 + i * 10)),
    \"precio\": $((100 + i * 20)),
    \"stock\": $((5 + i)),
    \"stockMinimo\": 3,
    \"descripcion\": \"Descripción producto $i\",
    \"proveedor\": \"Proveedor $i\"
  }" > /dev/null
  echo "  Producto $i creado"
done
# Segundo y tercer producto: id = PID1+1, PID1+2 (creados en orden)
PID2=$((PID1 + 1))
PID3=$((PID1 + 2))

echo ""
echo "=== 4. Crear servicios (óptica) ==="
S1=$(curl -s -X POST "$BASE/api/services" -H "Content-Type: application/json" -H "$AUTH" -d '{"nombreServicio":"Examen visual","precio":500,"descripcion":"Examen de agudeza visual"}')
S2=$(curl -s -X POST "$BASE/api/services" -H "Content-Type: application/json" -H "$AUTH" -d '{"nombreServicio":"Ajuste de montura","precio":150,"descripcion":"Ajuste y adaptación"}')
S3=$(curl -s -X POST "$BASE/api/services" -H "Content-Type: application/json" -H "$AUTH" -d '{"nombreServicio":"Limpieza de lentes","precio":80}')
IDS1=$(echo "$S1" | grep -o '"id":[0-9]*' | head -1 | grep -o '[0-9]*')
echo "Servicio 1 id=$IDS1"

echo ""
echo "=== 5. Venta en CÓRDOBAS (NIO) ==="
curl -s -X POST "$BASE/api/sales" -H "Content-Type: application/json" -H "$AUTH" -d "{
  \"clientId\": \"$IDC1\",
  \"clientName\": \"María López\",
  \"items\": [
    {\"type\":\"product\",\"productId\":$PID1,\"productName\":\"Producto Óptico 1\",\"quantity\":2,\"unitPrice\":120,\"subtotal\":240},
    {\"type\":\"service\",\"serviceId\":$IDS1,\"serviceName\":\"Examen visual\",\"quantity\":1,\"unitPrice\":500,\"subtotal\":500}
  ],
  \"total\": 740,
  \"amountPaid\": 740,
  \"paymentMethod\": \"efectivo\",
  \"currency\": \"NIO\",
  \"status\": \"completado\"
}" | head -c 300
echo ""

echo ""
echo "=== 6. Venta en DÓLARES (USD) ==="
curl -s -X POST "$BASE/api/sales" -H "Content-Type: application/json" -H "$AUTH" -d "{
  \"clientId\": \"$IDC2\",
  \"clientName\": \"Carlos Ruiz\",
  \"items\": [
    {\"type\":\"product\",\"productId\":$PID2,\"productName\":\"Producto Óptico 2\",\"quantity\":1,\"unitPrice\":25.50,\"subtotal\":25.50},
    {\"type\":\"product\",\"productId\":$PID3,\"productName\":\"Producto Óptico 3\",\"quantity\":1,\"unitPrice\":30,\"subtotal\":30}
  ],
  \"total\": 55.50,
  \"amountPaid\": 60,
  \"paymentMethod\": \"tarjeta\",
  \"currency\": \"USD\",
  \"status\": \"completado\"
}" | head -c 300
echo ""

echo ""
echo "=== 7. Cotización (sin cobro) ==="
curl -s -X POST "$BASE/api/sales" -H "Content-Type: application/json" -H "$AUTH" -d "{
  \"clientId\": \"$IDC1\",
  \"clientName\": \"María López - Cotización\",
  \"items\": [
    {\"type\":\"product\",\"productId\":$PID1,\"productName\":\"Producto Óptico 1\",\"quantity\":1,\"unitPrice\":120,\"subtotal\":120},
    {\"type\":\"service\",\"serviceId\":$IDS1,\"serviceName\":\"Examen visual\",\"quantity\":1,\"unitPrice\":500,\"subtotal\":500}
  ],
  \"total\": 620,
  \"amountPaid\": 0,
  \"currency\": \"NIO\",
  \"status\": \"cotizacion\"
}" | head -c 300
echo ""

echo ""
echo "=== 8. GET productos (paginado) ==="
curl -s -X GET "$BASE/api/products?page=1&pageSize=3" -H "$AUTH" | head -c 400
echo ""

echo ""
echo "=== 9. GET productos low-stock ==="
curl -s -X GET "$BASE/api/products/low-stock" -H "$AUTH" | head -c 350
echo ""

echo ""
echo "=== 10. GET clientes ==="
curl -s -X GET "$BASE/api/clients?page=1&pageSize=5" -H "$AUTH" | head -c 350
echo ""

echo ""
echo "=== 11. GET dashboard (resumen) ==="
curl -s -X GET "$BASE/api/dashboard/summary" -H "$AUTH" | head -c 350
echo ""

echo ""
echo "=== LISTO. Pruebas completadas. ==="
