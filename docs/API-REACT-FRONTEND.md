# API OptiControl – Guía para el frontend React

Documentación para integrar el **frontend React** con el **backend OptiControl**. Todas las rutas (excepto login) requieren autenticación con JWT.

---

## 1. Configuración base

| Concepto | Valor |
|----------|--------|
| **Base URL** | `https://tu-dominio.com` o `http://localhost:5000` (según entorno) |
| **Prefijo API** | `/api` |
| **Autenticación** | Bearer JWT en header en todas las peticiones excepto `POST /api/auth/login` |
| **Formato** | JSON (`Content-Type: application/json`) |

### Header en peticiones autenticadas

```http
Authorization: Bearer <token>
```

Si el backend responde **401**, el frontend debe borrar el token y redirigir a login.

---

## 2. Autenticación

### POST `/api/auth/login`

Iniciar sesión.

**Body (JSON):**
```json
{
  "nombreUsuario": "admin",
  "contrasena": "********"
}
```

**Respuesta 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": 1,
    "usuario": "admin",
    "nombreCompleto": "Administrador",
    "rol": "Administrador",
    "estado": "Activo"
  }
}
```

**Errores:** 400 (campos vacíos), 401 (credenciales incorrectas).

---

### GET `/api/auth/me`

Obtener usuario actual (validar token). Requiere `Authorization: Bearer <token>`.

**Respuesta 200:**
```json
{
  "id": 1,
  "usuario": "admin",
  "nombreCompleto": "Administrador",
  "rol": "Administrador",
  "estado": "Activo"
}
```

---

### POST `/api/auth/logout`

Cerrar sesión. Requiere token. El backend no invalida el token; el frontend debe borrarlo.

**Respuesta 200:** `{ "success": true }`

---

## 3. Dashboard

Base: **GET** `/api/dashboard/...`. Requiere rol **Administrador**.

### GET `/api/dashboard/summary`

Resumen general. Todos los totales de ingresos están en **córdobas** (las ventas en USD se convierten con el tipo de cambio configurado).

**Respuesta 200:**
```json
{
  "totalRevenue": 45000,
  "salesToday": 1200,
  "salesMonth": 8500,
  "productsCount": 587,
  "clientsCount": 15,
  "productsTotal": 20
}
```

---

### GET `/api/dashboard/recent-activity?limit=50`

Actividad reciente.

**Query:** `limit` (opcional, default 50).

**Respuesta 200:** array
```json
[
  {
    "id": "A1",
    "type": "sale",
    "description": "Venta registrada - María García · C$1,800",
    "time": "2026-03-10 14:30"
  }
]
```

`type`: `sale` | `client` | `product` | `inventory` | `service`.

---

### GET `/api/dashboard/monthly-income?months=12`

Ingresos por mes (en córdobas).

**Query:** `months` (opcional, default 12).

**Respuesta 200:** array
```json
[
  { "month": "Ene", "monthName": "Ene", "amount": 12000 },
  { "month": "Feb", "monthName": "Feb", "amount": 15000 }
]
```

---

### GET `/api/dashboard/top-products`

Productos/servicios más vendidos.

**Respuesta 200:** array
```json
[
  { "name": "Fundas para lentes", "quantity": 22 },
  { "name": "Examen visual", "quantity": 10 }
]
```

---

### GET `/api/dashboard/alerts`

Alertas (facturas vencidas, etc.). Opcional.

**Respuesta 200:** objeto con `overdueInvoices`, `message`, `upcomingTrips`, `upcomingTripsMessage`.

---

## 4. Inventario (Productos)

Base: **GET/POST/PUT/DELETE** `/api/products`. Las respuestas usan **snake_case**.

### GET `/api/products?page=1&pageSize=20&search=`

Listar productos (paginado y búsqueda por nombre o marca).

**Query:** `page`, `pageSize`, `search` (opcional).

**Respuesta 200:**
```json
{
  "items": [
    {
      "id": 1,
      "nombre_producto": "Montura clásica negra",
      "tipo_producto": "montura",
      "marca": "Ray-Ban",
      "precio_compra": 450,
      "precio": 850,
      "stock": 12,
      "fecha_creacion": "2025-12-05",
      "descripcion": "Montura metálica.",
      "proveedor": "Distribuidora Óptica"
    }
  ],
  "totalCount": 20,
  "totalPages": 1,
  "page": 1,
  "pageSize": 20
}
```

---

### GET `/api/products/:id`

Detalle de un producto. Misma forma que un elemento de `items` arriba.

---

### POST `/api/products`

Crear producto.

**Body (camelCase o snake_case según acepte el backend):**
```json
{
  "nombreProducto": "Montura clásica negra",
  "tipoProducto": "montura",
  "marca": "Ray-Ban",
  "precioCompra": 450,
  "precio": 850,
  "stock": 12,
  "fechaCreacion": "2025-12-05",
  "descripcion": "Montura metálica.",
  "proveedor": "Distribuidora Óptica"
}
```

`fechaCreacion` es opcional; si no se envía, el backend usa la fecha actual.

**Respuesta 201:** objeto producto creado (snake_case). Header `Location` con URL del recurso.

---

### PUT `/api/products/:id`

Actualizar producto. Mismos campos que el POST; `id` puede ir en la URL solamente.

**Respuesta 200:** objeto producto actualizado.

---

### DELETE `/api/products/:id`

Eliminar producto. **Respuesta 204** sin cuerpo.

---

## 5. Servicios (óptica)

Base: **GET/POST/PUT/DELETE** `/api/services` (controller `OpticsServicesController`).

### GET `/api/services?page=1&pageSize=20&search=`

Listar servicios. **Respuesta:** mismo formato paginado que productos; cada ítem:

```json
{
  "id": 1,
  "nombre_servicio": "Examen visual",
  "precio": 200,
  "descripcion": "Evaluación de la vista",
  "fecha_creacion": "2026-01-10"
}
```

### GET `/api/services/:id`

Detalle de un servicio.

### POST `/api/services`

**Body:** `nombre_servicio`, `precio`, `descripcion` (opcional), `fecha_creacion` (opcional).

### PUT `/api/services/:id`

Actualizar. Mismos campos.

### DELETE `/api/services/:id`

Eliminar. **Respuesta 204.**

---

## 6. Ventas (POS)

### POST `/api/sales`

Registrar **venta** o **cotización**.

**Body:**
```json
{
  "clientId": "1",
  "clientName": "María García",
  "items": [
    {
      "type": "product",
      "productId": 1,
      "productName": "Montura clásica negra",
      "quantity": 1,
      "unitPrice": 850,
      "subtotal": 850
    },
    {
      "type": "service",
      "serviceId": 1,
      "serviceName": "Examen visual",
      "quantity": 1,
      "unitPrice": 200,
      "subtotal": 200
    }
  ],
  "total": 1050,
  "amountPaid": 1050,
  "paymentMethod": "Efectivo",
  "currency": "NIO",
  "status": null
}
```

- **Venta:** no enviar `status` o enviar distinto de `"cotizacion"`. `amountPaid`: monto recibido. Si `amountPaid >= total` → venta **Pagada**; si no → **pendiente**.
- **Cotización:** enviar `"status": "cotizacion"`. No se descuenta stock; `amountPaid` puede ser 0.
- **Moneda:** `currency`: `"NIO"` (córdobas) o `"USD"` (dólares). El backend usa el tipo de cambio de configuración para totales en reportes.

**Respuesta 200:** venta creada, ej.:
```json
{
  "id": "V36",
  "date": "2026-03-12T10:00:00.000Z",
  "clientId": "1",
  "clientName": "María García",
  "items": [ ... ],
  "total": 1050,
  "amountPaid": 1050,
  "paymentMethod": "Efectivo",
  "currency": "NIO",
  "status": "Pagada"
}
```

**Error 400:** ej. "Stock insuficiente en uno o más productos."

---

## 7. Historial de ventas

Base: **GET/PUT** `/api/sales-history`.

### GET `/api/sales-history?page=1&pageSize=20`

Listar ventas y cotizaciones (paginado).

**Respuesta 200:**
```json
{
  "items": [
    {
      "id": "V1",
      "date": "2026-03-10T14:30:00",
      "clientId": "1",
      "clientName": "María García",
      "status": "Pagada",
      "total": 1800,
      "amountPaid": 1800,
      "paymentMethod": "Efectivo",
      "currency": "NIO",
      "items": [ ... ]
    }
  ],
  "totalCount": 35,
  "totalPages": 2,
  "page": 1,
  "pageSize": 20
}
```

`status`: `Pagada` | `pendiente` | `cotizacion` | `Cancelada`.

---

### GET `/api/sales-history/:id`

Detalle de una venta. El `id` en la URL es **numérico** (ej. `36`); en la respuesta el campo `id` puede ser `"V36"`.

---

### PUT `/api/sales-history/:id`

Cancelar venta o registrar abono.

**Cancelar:** body `{ "status": "Cancelada" }`. Solo si no está ya cancelada. Si era venta real (Pagada/pendiente), el backend devuelve stock.

**Abonar (venta pendiente):** body `{ "addPayment": 500 }`. Solo para ventas con `status` `pendiente`. Si `amountPaid` pasa a ser >= total, la venta queda **Pagada**.

**Respuesta 200:** venta actualizada (mismo objeto que GET por id).

**Errores 400:** "Venta no encontrada", "La venta ya está cancelada", "El monto a abonar debe ser mayor a 0", "Solo se puede abonar en ventas con estado Pendiente".

---

## 8. Clientes

Base: **GET/POST/PUT/DELETE** `/api/clients`.

### GET `/api/clients?page=1&pageSize=20&search=`

Listar clientes. **Query:** `page`, `pageSize`, `search` (nombre o teléfono).

**Respuesta 200:** paginada; cada ítem:
```json
{
  "id": 1,
  "name": "María García",
  "phone": "505 8123 4567",
  "address": "Managua, Barrio San Judas",
  "graduacion_od": "-1.50",
  "graduacion_oi": "-1.25",
  "fecha_registro": "2025-12-01",
  "email": "",
  "descripcion": "Cliente frecuente."
}
```

---

### GET `/api/clients/:id`

Detalle de un cliente.

---

### GET `/api/clients/:id/history?page=1&pageSize=10`

Historial del cliente (ventas, actividad, etc.).

**Respuesta 200:**
```json
{
  "client": { ... },
  "reservations": { "items": [], "totalCount": 0, "totalPages": 1, "page": 1, "pageSize": 10 },
  "sales": { "items": [ ... ], "totalCount": 5, "totalPages": 1, "page": 1, "pageSize": 10 },
  "invoices": { "items": [], "totalCount": 0, "totalPages": 1, "page": 1, "pageSize": 10 },
  "activity": [
    { "id": "h-1", "type": "sale", "description": "Venta V1 · C$1800", "time": "2026-03-10 14:30" }
  ]
}
```

---

### POST `/api/clients`

Crear cliente. Body con: `name`, `phone`, `address`, `graduacion_od`, `graduacion_oi`, `fecha_registro`, `email`, `descripcion`. `fecha_registro` opcional.

**Respuesta 201:** cliente creado (misma forma que un ítem del listado).

---

### PUT `/api/clients/:id`

Actualizar cliente. Mismos campos. **Respuesta 200:** cliente actualizado.

---

### DELETE `/api/clients/:id`

Eliminar cliente. **Respuesta 204.**

---

## 9. Egresos

Base: **GET/POST/PUT/DELETE** `/api/expenses`.

### GET `/api/expenses?page=1&pageSize=20&dateFrom=2026-01-01&dateTo=2026-03-31&category=Fijo`

Listar egresos. **Query:** `page`, `pageSize`, `dateFrom`, `dateTo` (YYYY-MM-DD), `category` (ej. Fijo, Marketing, Operativo).

**Respuesta 200:** paginada; cada ítem:
```json
{
  "id": 1,
  "date": "2025-12-02",
  "concept": "Alquiler local",
  "amount": 800,
  "category": "Fijo"
}
```

### GET `/api/expenses/:id`

Detalle de un egreso.

### POST `/api/expenses`

**Body:** `date`, `concept`, `amount`, `category`.

### PUT `/api/expenses/:id`

Actualizar. Mismos campos.

### DELETE `/api/expenses/:id`

Eliminar. **Respuesta 204.**

---

## 10. Reportes

Base: **GET** `/api/reports/...`. Los totales de ingresos están en **córdobas** (USD convertidos con el tipo de cambio).

### GET `/api/reports/ingresos-totales`

**Respuesta 200:** `{ "totalIncome", "salesToday", "salesMonth" }`

### GET `/api/reports/ventas-dia?dateFrom=2026-01-01&dateTo=2026-03-31&page=1&pageSize=20`

**Respuesta 200:** `{ "items", "totalCount", "totalPages", "page", "pageSize", "totalAmount" }` — `totalAmount` en córdobas.

### GET `/api/reports/ventas-mes?page=1&pageSize=20`

**Respuesta 200:** misma estructura; `totalAmount` en córdobas.

### GET `/api/reports/productos-mas-vendidos`

**Respuesta 200:** array `[ { "name", "quantity" }, ... ]`

### GET `/api/reports/sales?page=1&pageSize=20&dateFrom=&dateTo=`

**Respuesta 200:** `{ "items", "totalCount", "totalPages", "page", "pageSize", "totalAmountInCordobas" }`

---

## 11. Configuración

Base: **GET/PUT** `/api/settings`. Requiere **Administrador**. El **tipo de cambio (dólar)** lo configura aquí el dueño/admin.

### GET `/api/settings`

**Respuesta 200:**
```json
{
  "id": 1,
  "companyName": "OptiControl",
  "email": "contacto@opticontrol.com",
  "phone": "505 8123 4567",
  "address": "Managua, Nicaragua",
  "currency": "NIO",
  "language": "es",
  "exchangeRate": 36.8,
  "theme": "light",
  "soundVolume": 30,
  "alertsFacturasVencidas": true,
  "alertsRecordatorios": false,
  "alertsReservacionesPendientes": true,
  "updatedAt": "2026-03-12T10:00:00.000Z"
}
```

- **exchangeRate:** córdobas por 1 USD. El admin puede poner 37, 40, etc.; el backend usa este valor para convertir ventas en USD a C$ en totales.

### PUT `/api/settings`

Actualizar configuración. Se pueden enviar **solo los campos que cambian** (parcial). Ejemplo:

```json
{
  "companyName": "Mi Óptica",
  "exchangeRate": 37.5,
  "theme": "dark",
  "currency": "NIO"
}
```

**Respuesta 200:** objeto de configuración actualizado.

---

## 12. Actividad

### GET `/api/activity?limit=50`

Lista de actividad reciente (mismo formato que dashboard recent-activity).

**Respuesta 200:** array de `{ "id", "type", "description", "time" }`.

---

## 13. Paginación

En todos los listados paginados la respuesta incluye:

| Campo        | Tipo   | Descripción                    |
|-------------|--------|---------------------------------|
| `items`     | array  | Elementos de la página actual  |
| `totalCount`| number | Total de registros que cumplen |
| `totalPages`| number | Número total de páginas        |
| `page`      | number | Página actual (1-based)        |
| `pageSize`  | number | Tamaño de página usado        |

---

## 14. Errores

- **200:** OK (cuerpo según el endpoint).
- **201:** Created (recurso creado; revisar header `Location`).
- **204:** No Content (ej. DELETE correcto).
- **400:** Bad Request — body con mensaje, ej. `{ "error": "Stock insuficiente." }`.
- **401:** Unauthorized — token inválido o ausente. El frontend debe borrar token y redirigir a login.
- **404:** Not Found — recurso no existe; a veces `{ "error": "Venta no encontrada" }`.

El frontend puede mostrar `error` (o el mensaje que envíe el backend) en toast o alerta.

---

## 15. Resumen de rutas por módulo (React)

| Módulo        | Rutas principales |
|---------------|-------------------|
| Login         | `POST /api/auth/login`, `GET /api/auth/me`, `POST /api/auth/logout` |
| Dashboard     | `GET /api/dashboard/summary`, `recent-activity`, `monthly-income`, `top-products`, `alerts` |
| Inventario    | `GET|POST|PUT|DELETE /api/products` |
| Servicios     | `GET|POST|PUT|DELETE /api/services` |
| Ventas (POS)  | `POST /api/sales` |
| Historial     | `GET /api/sales-history`, `GET /api/sales-history/:id`, `PUT /api/sales-history/:id` |
| Clientes      | `GET|POST|PUT|DELETE /api/clients`, `GET /api/clients/:id/history` |
| Egresos       | `GET|POST|PUT|DELETE /api/expenses` |
| Reportes      | `GET /api/reports/ingresos-totales`, `ventas-dia`, `ventas-mes`, `productos-mas-vendidos`, `sales` |
| Configuración | `GET /api/settings`, `PUT /api/settings` |
| Actividad     | `GET /api/activity` |

Con esta documentación el frontend React puede adaptarse al backend OptiControl (auth, dashboard, productos, servicios, ventas, historial, clientes, egresos, reportes, configuración y actividad).
