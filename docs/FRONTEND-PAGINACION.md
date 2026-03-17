# Paginación en la API TripPilot

Todos los listados GET que devuelven colecciones usan ahora **paginación**. El frontend debe enviar `page` y `pageSize` (opcionales) y esperar la nueva forma de respuesta.

---

## 1. Parámetros de consulta

| Parámetro  | Tipo   | Por defecto | Descripción |
|------------|--------|--------------|-------------|
| **page**   | int    | 1            | Número de página (desde 1). |
| **pageSize** | int  | 20           | Cantidad de ítems por página. El backend limita el máximo a **100**. |

Ejemplo: `GET /api/clients?search=maria&page=2&pageSize=10`

---

## 2. Forma de la respuesta paginada

Los endpoints de listado devuelven un objeto con:

| Campo         | Tipo   | Descripción |
|---------------|--------|-------------|
| **items**     | array  | Ítems de la página actual. |
| **totalCount**| number | Total de registros que cumplen el filtro (todas las páginas). |
| **page**      | number | Página actual. |
| **pageSize**  | number | Tamaño de página usado. |
| **totalPages**| number | Cantidad total de páginas (`ceil(totalCount / pageSize)`). |

En algunos endpoints se añaden totales en córdobas u otros datos; se detallan más abajo.

---

## 3. Endpoints con paginación

### 3.1 Clientes

**GET** `/api/clients`

**Query:** `search` (opcional), `page`, `pageSize`

**Respuesta 200:**
```json
{
  "items": [ { "id": 1, "pasaporte": "...", "name": "...", "email": "...", "phone": "...", "status": "...", "lastTrip": null, ... } ],
  "totalCount": 72,
  "page": 1,
  "pageSize": 20,
  "totalPages": 4
}
```

---

### 3.2 Reservaciones

**GET** `/api/reservations`

**Query:** `clientId`, `paymentStatus`, `paymentMethod`, `dateFrom`, `dateTo`, `page`, `pageSize`

**Respuesta 200:** Mismo esquema (`items`, `totalCount`, `page`, `pageSize`, `totalPages`). Cada ítem es una reservación con `client` incluido.

---

### 3.3 Ventas

**GET** `/api/sales`

**Query:** `clientId`, `status`, `paymentMethod`, `dateFrom`, `dateTo`, `page`, `pageSize`

**Respuesta 200:** Incluye los totales calculados sobre **todos** los registros que cumplan el filtro (no solo la página):

```json
{
  "items": [ ... ],
  "totalCount": 180,
  "page": 1,
  "pageSize": 20,
  "totalPages": 9,
  "totalAmountInCordobas": 297315.00,
  "totalPendingInCordobas": 0
}
```

---

### 3.4 Facturas

**GET** `/api/invoices`

**Query:** `clientId`, `status`, `paymentMethod`, `dateFrom`, `dateTo`, `page`, `pageSize`

**Respuesta 200:**
```json
{
  "items": [ ... ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

---

### 3.5 Egresos

**GET** `/api/expenses`

**Query:** `dateFrom`, `dateTo`, `category`, `page`, `pageSize`

**Respuesta 200:** Mismo esquema (`items`, `totalCount`, `page`, `pageSize`, `totalPages`).

---

### 3.6 Historial de cliente

**GET** `/api/clients/{id}/history`

**Query (nuevos):** `page` (default 1), `pageSize` (default 10)

**Respuesta 200:** Las secciones **Reservations**, **Sales** e **Invoices** pasan a ser objetos paginados (cada una con su propia página y totales):

```json
{
  "client": { "id": 1, "name": "...", ... },
  "reservations": {
    "items": [ ... ],
    "totalCount": 5,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
  },
  "sales": {
    "items": [ ... ],
    "totalCount": 12,
    "page": 1,
    "pageSize": 10,
    "totalPages": 2
  },
  "invoices": {
    "items": [ ... ],
    "totalCount": 8,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
  },
  "activity": [ ... ]
}
```

- **activity** sigue siendo un array (sin paginación; límite interno 50).
- Para mostrar más reservaciones, ventas o facturas del mismo cliente, el frontend debe llamar de nuevo con `page=2` (o el número que corresponda) para la sección que quiera ampliar.

---

### 3.7 Reportes (listados por rango de fechas)

Estos GETs devuelven listas paginadas y totales calculados sobre el rango completo:

| Endpoint | Query adicional | Totales en la respuesta |
|----------|------------------|--------------------------|
| **GET** `/api/reports/sales` | `dateFrom`, `dateTo`, `page`, `pageSize` | `totalAmount` |
| **GET** `/api/reports/invoices` | `dateFrom`, `dateTo`, `page`, `pageSize` | `totalInvoiced` |
| **GET** `/api/reports/reservations` | `dateFrom`, `dateTo`, `page`, `pageSize` | `totalAmountInCordobas` |
| **GET** `/api/reports/expenses` | `dateFrom`, `dateTo`, `page`, `pageSize` | `totalAmount` |

Ejemplo **GET** `/api/reports/sales?dateFrom=2025-11-01&dateTo=2026-02-28&page=1&pageSize=20`:

```json
{
  "dateFrom": "2025-11-01T00:00:00Z",
  "dateTo": "2026-02-28T00:00:00Z",
  "items": [ ... ],
  "totalCount": 180,
  "page": 1,
  "pageSize": 20,
  "totalPages": 9,
  "totalAmount": 297315.00
}
```

---

## 4. Resumen para el frontend

1. **En todos los listados** (clientes, reservaciones, ventas, facturas, egresos, historial de cliente, reportes): enviar `page` y `pageSize` en la query cuando se use paginación (por defecto `page=1`, `pageSize=20`; en historial `pageSize=10`).
2. **Dejar de asumir** que la respuesta es un array directo: la lista está en `response.items`; usar `response.totalCount`, `response.page`, `response.totalPages` para la UI de paginación.
3. **Ventas** (`/api/sales`): seguir usando `totalAmountInCordobas` y `totalPendingInCordobas` para resúmenes; ya están calculados sobre todos los registros filtrados.
4. **Historial de cliente** (`/api/clients/{id}/history`): reservaciones, ventas y facturas vienen en `reservations.items`, `sales.items`, `invoices.items`, cada uno con su `totalCount`, `page`, `pageSize`, `totalPages` para poder paginar cada pestaña por separado.

---

## 5. Exportación (Excel / PDF)

Los endpoints de **export** (por ejemplo `GET /api/clients/export/excel`, `GET /api/sales/export/pdf`) **no** están paginados: generan el archivo con todos los registros que cumplan los filtros actuales.
