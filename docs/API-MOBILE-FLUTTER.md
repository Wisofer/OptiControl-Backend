# API Móvil OptiControl (Flutter)

Versión **reducida** de la API para la app móvil. Misma autenticación (JWT), respuestas más ligeras (menos campos, camelCase).

---

## Autenticación (compartida)

Usa los mismos endpoints que la web:

| Método | Ruta | Uso |
|--------|------|-----|
| POST | `/api/auth/login` | Body: `{ "nombreUsuario", "contrasena" }` → `{ "token", "user" }` |
| GET | `/api/auth/me` | Header: `Authorization: Bearer <token>` |
| POST | `/api/auth/logout` | Cerrar sesión |

---

## Endpoints móvil (`/api/mobile/...`)

Todos requieren: `Authorization: Bearer <token>`.

### 1. Resumen (home)

```
GET /api/mobile/summary
```

**Respuesta:**
```json
{
  "totalRevenue": 45000,
  "salesToday": 1200,
  "salesMonth": 8500,
  "productsCount": 587,
  "clientsCount": 15
}
```

---

### 2. Productos (lite)

```
GET /api/mobile/products?page=1&pageSize=50&search=
```

**Respuesta:** Solo campos necesarios para POS/listado.

```json
{
  "items": [
    { "id": 1, "name": "Montura clásica", "price": 850, "stock": 12, "type": "montura" }
  ],
  "totalCount": 20,
  "page": 1,
  "pageSize": 50
}
```

---

### 3. Servicios (lite)

```
GET /api/mobile/services?page=1&pageSize=50&search=
```

**Respuesta:**
```json
{
  "items": [
    { "id": 1, "name": "Examen visual", "price": 200 }
  ],
  "totalCount": 10,
  "page": 1,
  "pageSize": 50
}
```

---

### 4. Clientes (lite)

```
GET /api/mobile/clients?page=1&pageSize=50&search=
```

**Respuesta:** Para selector en ventas.

```json
{
  "items": [
    { "id": 1, "name": "María García", "phone": "505 8123 4567" }
  ],
  "totalCount": 15,
  "page": 1,
  "pageSize": 50
}
```

---

### 5. Registrar venta

```
POST /api/mobile/sales
```

**Body:** Igual que `POST /api/sales` (venta o cotización).

```json
{
  "clientId": "1",
  "clientName": "María García",
  "items": [
    { "type": "product", "productId": 1, "productName": "Montura", "quantity": 1, "unitPrice": 850, "subtotal": 850 },
    { "type": "service", "serviceId": 1, "serviceName": "Examen", "quantity": 1, "unitPrice": 200, "subtotal": 200 }
  ],
  "total": 1050,
  "amountPaid": 1050,
  "paymentMethod": "Efectivo",
  "currency": "NIO",
  "status": null
}
```

**Respuesta:** Objeto de la venta creada (mismo formato que web).

---

### 6. Ventas recientes

```
GET /api/mobile/sales/recent?limit=20
```

**Respuesta:** Lista reducida para historial rápido.

```json
{
  "items": [
    { "id": "V36", "date": "2026-03-12T...", "clientName": "María García", "total": 1050, "status": "Pagada" }
  ]
}
```

---

### 7. Configuración mínima

```
GET /api/mobile/settings
```

**Respuesta:**
```json
{
  "companyName": "OptiControl",
  "currency": "NIO",
  "exchangeRate": 36.8
}
```

---

## Resumen: qué usa la versión móvil

| Funcionalidad | Web (React) | Móvil (Flutter) |
|---------------|-------------|------------------|
| Login / Auth | ✅ | ✅ (mismo) |
| Dashboard completo | ✅ summary, activity, monthly, top-products, alerts | ✅ Solo **summary** |
| Inventario | ✅ CRUD completo, todos los campos | ✅ Lista **lite** (id, name, price, stock, type) |
| Servicios | ✅ CRUD completo | ✅ Lista **lite** (id, name, price) |
| Clientes | ✅ CRUD + historial | ✅ Lista **lite** (id, name, phone) para selector |
| Registrar venta | ✅ POST /api/sales | ✅ POST /api/mobile/sales |
| Historial ventas | ✅ Listado completo, paginado, cancelar, abonos | ✅ Solo **recent** (últimas N) |
| Egresos | ✅ CRUD | ❌ No en móvil |
| Reportes | ✅ Varios | ❌ No en móvil |
| Configuración | ✅ GET/PUT completo | ✅ Solo **GET** mínima (empresa, moneda) |
| Actividad reciente | ✅ | ❌ No en móvil |

La app Flutter puede usar **solo** `/api/auth/*` y `/api/mobile/*` y tener POS + resumen + ventas recientes + configuración básica.
