#!/bin/bash
# Pruebas API OptiControl: login, inventario, servicios, clientes, egresos, usuarios y plantillas
set -euo pipefail

BASE="http://localhost:5229"

month_name_from_mm() {
  case "$1" in
    "01") echo "Enero" ;;
    "02") echo "Febrero" ;;
    "03") echo "Marzo" ;;
    "04") echo "Abril" ;;
    "05") echo "Mayo" ;;
    "06") echo "Junio" ;;
    "07") echo "Julio" ;;
    "08") echo "Agosto" ;;
    "09") echo "Septiembre" ;;
    "10") echo "Octubre" ;;
    "11") echo "Noviembre" ;;
    "12") echo "Diciembre" ;;
    *) echo "" ;;
  esac
}

# Ventana movil: ultimos 3 meses (incluye el mes actual).
TODAY_FIRST="$(date +%Y-%m-01)"
MONTH_YM=(
  "$(date -d "$TODAY_FIRST -2 months" +%Y-%m)"
  "$(date -d "$TODAY_FIRST -1 months" +%Y-%m)"
  "$(date -d "$TODAY_FIRST" +%Y-%m)"
)
MONTH_YEARS=()
MONTHS=()
MONTH_NAMES=()
for ym in "${MONTH_YM[@]}"; do
  MONTH_YEARS+=("${ym%%-*}")
  mm="${ym##*-}"
  MONTHS+=("$mm")
  MONTH_NAMES+=("$(month_name_from_mm "$mm")")
done

json_get() {
  # Usa python para parsear JSON y evitar hacks con grep/cut.
  python3 - "$1" <<'PY'
import json,sys
data=json.load(sys.stdin)
path=sys.argv[1]
for part in path.strip().split('.'):
    if part.endswith(']'):
        # no usado en este script
        raise SystemExit("path con index no soportado")
    data=data.get(part)
    if data is None:
        break
print("" if data is None else data)
PY
}

login() {
  >&2 echo "=== 1. Login ==="
  local resp token
  resp="$(curl -s -X POST "$BASE/api/auth/login" \
    -H "Content-Type: application/json" \
    -d '{"nombreUsuario":"admin","contrasena":"admin"}')"
  # Si el backend no responde, $resp puede venir vacío y el JSON parse falla.
  if [ -z "$resp" ]; then
    >&2 echo "Error: sin respuesta del servidor en $BASE (¿está corriendo la API?)."
    >&2 echo "Consejo: verifica que haya un listener en el puerto $BASE (5229)."
    exit 1
  fi
  token="$(printf '%s' "$resp" | python3 -c 'import sys,json
s=sys.stdin.read()
try:
    data=json.loads(s) if s and s.strip() else {}
except Exception:
    data={}
print(data.get("token","") if isinstance(data,dict) else "")')"
  if [ -z "$token" ]; then
    >&2 echo "Login falló: respuesta=$(printf '%s' "$resp" | head -c 300)"
    exit 1
  fi
  >&2 echo "OK - Token obtenido"
  # Importante: stdout solo contiene el token (porque llamamos login() en command substitution).
  printf '%s' "$token"
}

AUTH_HEADER() {
  echo "Authorization: Bearer $1"
}

POST_JSON() {
  # POST_JSON URL JSON_BODY
  local url="$1"
  local body="$2"
  curl -s -X POST "$url" -H "Content-Type: application/json" -H "$AUTH" -d "$body"
}

extract_id_number() {
  # Extrae el campo "id" numérico del JSON.
  # Nota: si el backend devuelve id como string, igual intentamos parsearlo.
  printf '%s' "$1" | python3 -c 'import sys,json,re
s=sys.stdin.read()
try:
    data=json.loads(s) if s and s.strip() else {}
except Exception:
    data={}
v=data.get("id") if isinstance(data,dict) else None
print(re.sub(r"[^0-9]","", str(v)) if v is not None else "")'
}

echo "== Generando datos en ventana de 3 meses: ${MONTH_YM[0]} .. ${MONTH_YM[2]} =="

TOKEN="$(login)"
AUTH="$(AUTH_HEADER "$TOKEN")"

echo ""
echo "=== 1. Crear usuarios y plantilla WhatsApp ==="

ensure_user() {
  # ensure_user USERNAME PASSWORD NOMBRE_COMPLETO ROL
  local u="$1"
  local p="$2"
  local fullname="$3"
  local role="$4"

  local existing_id
  existing_id="$(curl -sS -X GET "$BASE/api/users" -H "$AUTH" | python3 -c 'import sys,json
s=sys.stdin.read()
if not s.strip():
    print("")
    raise SystemExit(0)
try:
    data=json.loads(s)
except Exception:
    print("")
    raise SystemExit(0)
needle=sys.argv[1]
try:
    it=next((str(x.get("id")) for x in data if (x.get("usuario") or "").lower()==needle.lower()),"")
except Exception:
    it=""
print(it)' "$u")"

  if [ -n "$existing_id" ]; then
    echo "Usuario $u ya existe (id=$existing_id)"
    return 0
  fi

  local body
  body="$(cat <<EOF
{
  "nombreUsuario": "$u",
  "contrasena": "$p",
  "nombreCompleto": "$fullname",
  "rol": "$role"
}
EOF
)"
  local resp
  resp="$(POST_JSON "$BASE/api/users" "$body")"
  local new_id
  new_id="$(extract_id_number "$resp")"
  if [ -z "$new_id" ]; then
    echo "Aviso: no se pudo confirmar id para usuario $u. Respuesta: $(printf '%s' "$resp" | head -c 200)"
  else
    echo "Usuario $u creado (id=$new_id)"
  fi
}

ensure_whatsapp_template() {
  # ensure_whatsapp_template NOMBRE MENSAJE
  local nombre="$1"
  local mensaje="$2"

  local existing_id
  existing_id="$(curl -sS -X GET "$BASE/api/whatsapp-templates" -H "$AUTH" | python3 -c 'import sys,json
s=sys.stdin.read()
if not s.strip():
    print("")
    raise SystemExit(0)
try:
    data=json.loads(s)
except Exception:
    print("")
    raise SystemExit(0)
needle=sys.argv[1]
try:
    it=next((str(x.get("id")) for x in data if (x.get("nombre") or "").strip()==needle),"")
except Exception:
    it=""
print(it)' "$nombre")"

  if [ -n "$existing_id" ]; then
    echo "Plantilla WhatsApp '$nombre' ya existe (id=$existing_id)"
    return 0
  fi

  local body
  body="$(cat <<EOF
{
  "nombre": "$nombre",
  "mensaje": "$mensaje",
  "activa": true,
  "predeterminada": true
}
EOF
)"
  local resp
  resp="$(POST_JSON "$BASE/api/whatsapp-templates" "$body")"
  local new_id
  new_id="$(extract_id_number "$resp")"
  if [ -z "$new_id" ]; then
    echo "Aviso: no se pudo confirmar id para plantilla. Respuesta: $(printf '%s' "$resp" | head -c 200)"
  else
    echo "Plantilla WhatsApp creada (id=$new_id)"
  fi
}

# Usuarios: si ya existen, se omiten (idempotente por nombre de usuario).
ensure_user "admin" "admin" "Administrador del Sistema" "Administrador"
ensure_user "mlopez" "usuario" "Maria Lopez" "Usuario"
ensure_user "cmartinez" "usuario" "Carlos Martinez" "Usuario"
ensure_user "kgonzalez" "usuario" "Karen Gonzalez" "Administrador"

TEMPLATE_NAME="Factura con enlace PDF"
TEMPLATE_MSG="Hola {NombreCliente}, su factura {NumeroFactura} por {Monto} esta {Estado}. Mes: {Mes}. Categoria: {Categoria}. Puede descargar el PDF aqui: {EnlacePDF}. Gracias por confiar en nosotros."
ensure_whatsapp_template "$TEMPLATE_NAME" "$TEMPLATE_MSG"

echo ""
echo "=== 2. Crear productos (inventario) ==="
# Creamos una base de productos; las ventas referencian estos IDs.
# Body según backend: nombreProducto, tipoProducto, precioCompra, stockMinimo, fechaCreacion.
PRODUCT_NAMES=(
  "Montura clásica negra"
  "Montura elegante dorada"
  "Lentes oftálmicos antirreflejo"
  "Lentes para sol polarizados"
  "Accesorio: estuche rígido"
)
PRODUCT_TYPES=( "montura" "montura" "lente" "lente" "accesorio" )
PRODUCT_BRANDS=( "Ray-Ban" "Oakley" "Hoya" "Essilor" "SunOptic" )
PRODUCT_PROVEEDORES=(
  "Óptica Central S.A."
  "Distribuidora Visión"
  "Laboratorio Hoya Centro"
  "Representaciones Essilor"
  "Comercial SunOptic"
)
PRODUCT_PRICES_COMPRA=( 60 85 35 55 18 )
PRODUCT_PRICES_VENTA=( 140 190 75 130 35 )
PRODUCT_STOCK=( 22 16 30 18 40 )
PRODUCT_DESCS=(
  "Montura metálica clásica, ajuste cómodo."
  "Montura ligera con acabado premium."
  "Tratamiento antirreflejo para mayor nitidez."
  "Polarizado para reducir deslumbramiento."
  "Estuche rígido para protección y transporte."
)

PRODUCT_IDS=()
for i in 0 1 2 3 4; do
  mi_idx=$(( i % 3 ))
  year_i="${MONTH_YEARS[$mi_idx]}"
  month_i="${MONTHS[$mi_idx]}"
  day_i="$(printf '%02d' $((11 + i)))"
  date_creacion="${year_i}-${month_i}-${day_i}"
  body="$(cat <<EOF
{
  "nombreProducto": "${PRODUCT_NAMES[$i]}",
  "tipoProducto": "${PRODUCT_TYPES[$i]}",
  "marca": "${PRODUCT_BRANDS[$i]}",
  "precioCompra": ${PRODUCT_PRICES_COMPRA[$i]},
  "precio": ${PRODUCT_PRICES_VENTA[$i]},
  "stock": ${PRODUCT_STOCK[$i]},
  "stockMinimo": 5,
  "descripcion": "${PRODUCT_DESCS[$i]}",
  "proveedor": "${PRODUCT_PROVEEDORES[$i]}",
  "fechaCreacion": "${date_creacion}"
}
EOF
)"
  resp="$(POST_JSON "$BASE/api/products" "$body")"
  pid="$(extract_id_number "$resp")"
  if [ -z "$pid" ]; then
    echo "No se pudo obtener id de producto. Respuesta: $(printf '%s' "$resp" | head -c 250)"
    exit 1
  fi
  PRODUCT_IDS+=("$pid")
  echo "Producto '${PRODUCT_NAMES[$i]}' id=$pid (fecha=$date_creacion)"
done

echo ""
echo "=== 3. Crear servicios ==="
SERVICE_IDS=()
SERV_NAMES=( "Examen de agudeza visual" "Ajuste y adaptación de montura" "Limpieza profesional de lentes" )
SERV_PRICES=( 450 180 95 )
SERV_DESCS=(
  "Evaluación de agudeza visual y registro de graduación."
  "Ajuste de medidas, adaptación y verificación de comodidad."
  "Limpieza especializada con insumos y secado adecuado."
)
for idx in 0 1 2; do
  year_s="${MONTH_YEARS[$idx]}"
  m="${MONTHS[$idx]}"
  day_s=$((5 + idx))
  day_s="$(printf '%02d' "$day_s")"
  date_creacion="${year_s}-${m}-${day_s}"
  name="${SERV_NAMES[$idx]}"
  price="${SERV_PRICES[$idx]}"
  desc="${SERV_DESCS[$idx]}"
  body="$(cat <<EOF
{
  "nombreServicio": "$name",
  "precio": $price,
  "descripcion": "$desc",
  "fechaCreacion": "$date_creacion"
}
EOF
)"
  resp="$(POST_JSON "$BASE/api/services" "$body")"
  sid="$(extract_id_number "$resp")"
  if [ -z "$sid" ]; then
    echo "No se pudo obtener id de servicio. Respuesta: $(printf '%s' "$resp" | head -c 250)"
    exit 1
  fi
  SERVICE_IDS+=("$sid")
  echo "Servicio ${name} id=$sid (fecha=$date_creacion)"
done

echo ""
echo "=== 4. Crear clientes (historial por fechas) ==="
CLIENT_IDS=()
CLIENT_NAMES=()
CLIENT_DATA=(
  "Juan Martín|juan.martin@email.com|505 8123 4567|Managua, Barrio San Judas|-1.75|-1.50|Cliente frecuente"
  "María Elena García|maria.elena.garcia@email.com|505 8876 1234|Managua, Residencial Bello Horizonte|-2.25|-2.00|Control de graduación"
  "Carlos Alberto Pérez|carlos.perez@email.com|505 8234 5678|Masaya, Centro|-0.75|-0.50|Cambio de montura"
  "Sofía Navarro|sofia.navarro@email.com|505 8345 6789|Granada, Barrio Xochimilco|-3.00|-2.75|Seguimiento de lentes"
  "Diego Rodríguez|diego.rodriguez@email.com|505 8456 7890|León, Barrio El Calvario|-1.25|-1.00|Servicio post-venta"
)

for i in 0 1 2 3 4; do
  mi_idx=$(( i % 3 ))
  year_c="${MONTH_YEARS[$mi_idx]}"
  month_i="${MONTHS[$mi_idx]}"
  # i 0..4 => 04..08
  day_c=$((3 + i + 1))
  day_c="$(printf '%02d' "$day_c")"
  date_reg="${year_c}-${month_i}-${day_c}"
  IFS='|' read -r cname email phone address od oi desc <<<"${CLIENT_DATA[$i]}"
  body="$(cat <<EOF
{
  "name": "$cname",
  "email": "$email",
  "phone": "$phone",
  "address": "$address",
  "graduacionOd": "$od",
  "graduacionOi": "$oi",
  "fechaRegistro": "$date_reg",
  "descripcion": "$desc"
}
EOF
)"
  # Nota: el backend puede ignorar campos si no coinciden; por eso validamos id.
  resp="$(POST_JSON "$BASE/api/clients" "$body")"
  cid="$(extract_id_number "$resp")"
  if [ -z "$cid" ]; then
    echo "No se pudo obtener id de cliente. Respuesta: $(printf '%s' "$resp" | head -c 250)"
    exit 1
  fi
  CLIENT_IDS+=("$cid")
  CLIENT_NAMES+=("$cname")
  echo "Cliente $cname id=$cid (fecha_registro=$date_reg)"
done

echo ""
echo "=== 5. Crear egresos ==="
EXP_CONCEPTS=(
  "Compra de alcohol isopropílico y paños"
  "Pago de internet y soporte oficina"
  "Alquiler del local"
  "Publicidad y anuncios en redes (marketing)"
  "Mantenimiento de aire acondicionado"
  "Compra de repuestos menores (tornillería)"
)
EXP_CATEGORIES=( "Operativo" "Fijo" "Fijo" "Marketing" "Operativo" "Operativo" )
EXP_AMOUNTS=( 125 185 650 220 300 95 )

BODY_TMP=""
e_idx=0
for mi in 0 1 2; do
  m="${MONTHS[$mi]}"
  year_e="${MONTH_YEARS[$mi]}"
  mname="${MONTH_NAMES[$mi]}"
  for j in 1 2; do
    date_exp="${year_e}-${m}-$((12 + j * 4))"
    # mi 0..2, j 1..2 => 0..5
    e_idx=$((mi*2 + (j-1)))
    category="${EXP_CATEGORIES[$e_idx]}"
    amount="${EXP_AMOUNTS[$e_idx]}"
    concept="${EXP_CONCEPTS[$e_idx]}"
    body="$(cat <<EOF
{
  "concept": "$concept",
  "amount": $amount,
  "date": "$date_exp",
  "category": "$category"
}
EOF
)"
    POST_JSON "$BASE/api/expenses" "$body" >/dev/null
    echo "Egreso ($mname) ok: $concept=$amount (date=$date_exp)"
  done
done

echo ""
echo "=== LISTO. Inventario, servicios, clientes, egresos, usuarios y plantilla WhatsApp creados. ==="
