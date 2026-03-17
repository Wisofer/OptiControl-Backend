# Especificación frontend: Módulo de Egresos

Documento para que el equipo de frontend implemente la pantalla y flujos de **Egresos** en TripPilot. El backend ya expone la API; aquí se define qué debe hacer la UI.

---

## 1. Resumen

- **Módulo:** Egresos (gastos de la agencia).
- **Base URL API:** `GET/POST /api/expenses` y `GET/PUT/DELETE /api/expenses/{id}`.
- **Autenticación:** Todas las rutas requieren `Authorization` (token JWT/Bearer), igual que el resto de la app.

---

## 2. Modelo de datos (Egreso)

| Campo     | Tipo    | Requerido (crear/editar) | Descripción                                      |
|----------|---------|---------------------------|--------------------------------------------------|
| `id`     | number  | solo lectura              | ID del egreso (asignado por el backend).         |
| `date`   | string  | no (default: hoy)         | Fecha del egreso en formato `YYYY-MM-DD`.        |
| `concept`| string  | **sí**                    | Descripción del gasto (ej: "Pago proveedor").    |
| `amount` | number  | **sí**                    | Monto (decimal, positivo).                       |
| `category` | string | no (default: "Operativo") | Una de: `"Operativo"` \| `"Fijo"` \| `"Marketing"`. |

---

## 3. Endpoints a usar

| Acción   | Método | URL                      | Body (JSON) |
|----------|--------|--------------------------|-------------|
| Listar   | GET    | `/api/expenses`          | —           |
| Filtros  | GET    | `/api/expenses?dateFrom=YYYY-MM-DD&dateTo=YYYY-MM-DD&category=Operativo` | — |
| Ver uno  | GET    | `/api/expenses/{id}`     | —           |
| Crear    | POST   | `/api/expenses`          | `{ date?, concept, amount, category? }` |
| Actualizar | PUT  | `/api/expenses/{id}`     | `{ id, date, concept, amount, category }` |
| Eliminar | DELETE | `/api/expenses/{id}`     | —           |

- **Crear:** responde **201** con el egreso creado (incluye `id`). **400** si faltan `concept` o `amount` o son inválidos.
- **Actualizar:** **200** con el egreso actualizado. **400** si el `id` del body no coincide con la URL. **404** si no existe.
- **Eliminar:** **204** sin body. **404** si no existe.

---

## 4. Qué debe implementar el frontend

### 4.1 Pantalla principal: Listado de egresos

- **Ruta sugerida:** por ejemplo `/egresos` o `/expenses`.
- **Contenido:**
  - Tabla o lista de egresos con: fecha, concepto, monto, categoría.
  - Filtros opcionales:
    - **Rango de fechas:** `dateFrom`, `dateTo` (enviar en formato `YYYY-MM-DD`).
    - **Categoría:** dropdown/select con `Operativo`, `Fijo`, `Marketing` (y opción “Todas” = no enviar `category`).
  - Botón/acción **“Nuevo egreso”** que abra el formulario de alta.
  - Por cada fila en la columna **Acciones**: un botón/ícono **Ver** (ojito/eye) que abra un modal o panel con el detalle del egreso (solo lectura), y las acciones **Editar** y **Eliminar** (con confirmación antes de borrar).

#### Acción "Ver" (ícono de ojo)

- Al hacer clic en el ícono de **ver** (ojito), llamar a `GET /api/expenses/{id}` y mostrar en un **modal** o **panel lateral** la información del egreso en solo lectura:
  - **Código:** EGR-{id con ceros a la izquierda, ej. EGR-0060}
  - **Fecha**
  - **Descripción / Concepto**
  - **Categoría**
  - **Proveedor** (si el frontend lo maneja)
  - **Monto** (formato moneda, ej. C$ 670)
- Igual que en otras pantallas (clientes, facturas, etc.), el ojito solo abre el detalle sin permitir editar; para editar se usa la acción Editar.

### 4.2 Formulario: Crear / Editar egreso

- **Crear:** modal o pantalla con campos:
  - **Concepto** (requerido).
  - **Monto** (requerido, número > 0).
  - **Fecha** (opcional; si no se envía, el backend usa la fecha actual).
  - **Categoría** (opcional; select: Operativo, Fijo, Marketing; default Operativo).
- **Editar:** mismos campos, rellenados con los datos del egreso; al guardar enviar **PUT** con `id` en la URL y en el body.
- Validar en frontend que `concept` no esté vacío y que `amount` sea numérico y positivo antes de llamar a la API.

### 4.3 Integración con reportes (opcional pero recomendable)

- **Ingresos vs Egresos:**  
  `GET /api/reports/income-vs-expenses?dateFrom=...&dateTo=...`  
  Respuesta: `{ totalIncome, totalExpenses, balance }`.  
  Útil para un resumen o tarjetas en dashboard o en la misma vista de egresos.

- **Reporte de egresos:**  
  `GET /api/reports/expenses?dateFrom=...&dateTo=...`  
  Respuesta: `{ items: [ ...egresos... ], totalAmount }`.  
  Útil para mostrar total del período en la pantalla de listado.

---

## 5. Ejemplos de request/response

**Listar (sin filtros):**
```http
GET /api/expenses
```
```json
[
  { "id": 1, "date": "2025-02-20", "concept": "Pago proveedor", "amount": 500, "category": "Operativo" },
  { "id": 2, "date": "2025-02-25", "concept": "Facebook Ads", "amount": 350, "category": "Marketing" }
]
```

**Crear:**
```http
POST /api/expenses
Content-Type: application/json
```
```json
{
  "date": "2025-02-20",
  "concept": "Pago proveedor de transporte",
  "amount": 500,
  "category": "Operativo"
}
```
Respuesta **201** con el mismo objeto más `id` asignado.

**Actualizar:**
```http
PUT /api/expenses/1
Content-Type: application/json
```
```json
{
  "id": 1,
  "date": "2025-02-20",
  "concept": "Pago proveedor de transporte (actualizado)",
  "amount": 550,
  "category": "Operativo"
}
```

---

## 6. Resumen de tareas para frontend

1. Crear ruta/pantalla **Listado de egresos** con tabla y filtros (`dateFrom`, `dateTo`, `category`).
2. En la columna **Acciones**, añadir botón **Ver** (ícono de ojo) que llame a `GET /api/expenses/{id}` y muestre el detalle del egreso en un modal (solo lectura).
3. Implementar **Crear egreso** (formulario + POST).
4. Implementar **Editar egreso** (formulario + PUT usando `id`).
5. Implementar **Eliminar egreso** (DELETE + confirmación).
6. Validar en UI: `concept` obligatorio, `amount` numérico y positivo.
7. (Opcional) Mostrar en la misma pantalla o en dashboard el resumen de ingresos vs egresos y/o total de egresos del período usando los endpoints de reportes indicados.

Con esto el frontend puede implementar el módulo de egresos de punta a punta usando la API ya disponible en el backend.
