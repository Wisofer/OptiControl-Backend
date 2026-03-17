# Especificación frontend: Plantillas WhatsApp y envío de factura por WhatsApp

Documento para que el equipo de frontend implemente la **gestión de plantillas de mensaje WhatsApp** y el flujo **“Enviar factura por WhatsApp”** (con enlace de descarga del PDF). El backend ya expone la API; aquí se define qué debe hacer la UI.

---

## 1. Resumen

- **Plantillas WhatsApp:** CRUD de plantillas de texto con variables que el frontend reemplaza al armar el mensaje para enviar por WhatsApp.
- **URL pública del PDF:** El backend genera el PDF de la factura y expone una URL pública (sin login) para descargarlo. Esa URL se usa en la plantilla como `{EnlacePDF}`.
- **Flujo:** Usuario elige una factura → “Enviar por WhatsApp” → se obtiene la plantilla y la URL del PDF → se reemplazan variables → se abre WhatsApp con el mensaje y el cliente puede descargar el PDF al pulsar el enlace.

**Autenticación:** Las rutas de plantillas y de “pdf-url” requieren **Authorization** (Bearer JWT). La descarga del PDF es **pública** (no requiere token).

---

## 2. Modelo de datos: Plantilla WhatsApp

| Campo          | Tipo    | Requerido (crear/editar) | Descripción |
|----------------|---------|---------------------------|-------------|
| `id`           | number  | solo lectura              | ID único. |
| `nombre`       | string  | **sí** (crear)            | Ej: "Por Defecto", "Recordatorio pago". |
| `mensaje`      | string  | **sí** (crear)            | Texto con variables (ver sección 4). |
| `activa`       | boolean | no (default true)        | Si está disponible para usar. |
| `predeterminada` | boolean | no (default false)      | Solo una plantilla puede ser predeterminada. |

---

## 3. Endpoints a usar

### 3.1 Plantillas WhatsApp (solo Administrador)

| Acción     | Método | URL |
|------------|--------|-----|
| Listar     | GET    | `/api/whatsapp-templates` |
| Solo activas| GET    | `/api/whatsapp-templates?onlyActive=true` |
| Predeterminada | GET | `/api/whatsapp-templates/default` |
| Una plantilla | GET | `/api/whatsapp-templates/{id}` |
| Crear      | POST   | `/api/whatsapp-templates` |
| Actualizar | PUT    | `/api/whatsapp-templates/{id}` |
| Eliminar   | DELETE | `/api/whatsapp-templates/{id}` |

**Crear (POST)**  
Body: `{ "nombre": "...", "mensaje": "...", "activa": true, "predeterminada": false }`  
Requeridos: `nombre`, `mensaje`.  
Respuesta **201** con la plantilla creada. Si `predeterminada: true`, el backend desmarca la anterior.

**Actualizar (PUT)**  
Body: `{ "nombre": "...", "mensaje": "...", "activa": true, "predeterminada": false }` (todos opcionales). Misma lógica de “solo una predeterminada”.

**Eliminar (DELETE)**  
**204** si ok. **400** si se intenta eliminar la única plantilla predeterminada (hay que asignar otra antes).

### 3.2 URL del PDF para la factura (usuario autenticado)

| Acción | Método | URL |
|--------|--------|-----|
| Obtener URL del PDF | GET | `/api/invoices/{id}/pdf-url` |

Ejemplo de factura `id`: `INV-001`.  
Respuesta **200**: `{ "pdfUrl": "https://trippilot.cowib.es/api/public/invoices/INV-001/pdf" }`.  
Esa URL es la que se usa para reemplazar `{EnlacePDF}` en la plantilla.

### 3.3 Descarga pública del PDF (sin autenticación)

| Acción | Método | URL |
|--------|--------|-----|
| Descargar PDF | GET | `/api/public/invoices/{id}/pdf` |

No se envía header `Authorization`. Al abrir la URL en el navegador (o desde WhatsApp) se descarga el archivo `Factura-{id}.pdf`.  
**404** si la factura no existe.

---

## 4. Variables del mensaje (reemplazo en frontend)

El backend solo guarda el texto con placeholders. El **frontend** debe reemplazarlos con los datos de la factura y del cliente al armar el mensaje para WhatsApp.

| Variable        | Origen sugerido | Ejemplo |
|-----------------|------------------|---------|
| `{NombreCliente}` | Factura → cliente → nombre | "Carlos López" |
| `{CodigoCliente}` | Factura → cliente → id o código | "2" |
| `{NumeroFactura}` | Factura → id | "INV-001" |
| `{Monto}`        | Factura → amount (formateado) | "1,000.00" |
| `{Mes}`          | Factura → date (mes) | "Febrero" |
| `{Categoria}`     | Concepto o categoría si existe | "Paquete España" |
| `{Estado}`        | Factura → status | "Pendiente" / "Pagado" |
| `{FechaCreacion}` | Factura → date (formateado) | "27/02/2026" |
| `{EnlacePDF}`     | **GET** `/api/invoices/{id}/pdf-url` → `pdfUrl` | URL completa del PDF |

No es obligatorio usar todas; se reemplazan las que aparezcan en la plantilla.

---

## 5. Qué debe implementar el frontend

### 5.1 Pantalla: Gestión de plantillas WhatsApp (solo Administrador)

- **Ruta sugerida:** por ejemplo `/configuracion/plantillas-whatsapp` o dentro de Configuración.
- **Contenido:**
  - Listado de plantillas: nombre, mensaje (resumido o preview), activa, predeterminada.
  - Filtro opcional: solo activas (`?onlyActive=true`).
  - Botón **“Nueva plantilla”**.
  - Por cada fila: **Editar** y **Eliminar** (confirmación antes de borrar; si es la única predeterminada, mostrar mensaje de error del backend o avisar antes).
- **Formulario crear/editar:**
  - **Nombre** (requerido).
  - **Mensaje** (requerido, textarea; indicar que pueden usar las variables listadas).
  - **Activa** (checkbox, default true).
  - **Predeterminada** (checkbox; avisar que solo puede haber una).
- Al guardar: POST (crear) o PUT (editar) con el body correspondiente.

### 5.2 Flujo: “Enviar factura por WhatsApp”

Desde la pantalla de **Facturas** (o detalle de factura):

1. Usuario hace clic en **“Enviar por WhatsApp”** (o similar) para una factura.
2. **Obtener plantilla:**  
   `GET /api/whatsapp-templates/default` (o permitir elegir otra con `GET /api/whatsapp-templates?onlyActive=true`).
3. **Obtener URL del PDF:**  
   `GET /api/invoices/{id}/pdf-url` → guardar `pdfUrl` para `{EnlacePDF}`.
4. **Armar mensaje:** Reemplazar en el texto de la plantilla:
   - `{EnlacePDF}` → valor de `pdfUrl`.
   - `{NombreCliente}`, `{NumeroFactura}`, `{Monto}`, `{Estado}`, `{FechaCreacion}`, etc., con datos de la factura y del cliente (ya disponibles en la lista/detalle).
5. **Abrir WhatsApp:** Construir enlace  
   `https://wa.me/{telefono}?text={mensajeCodificado}`  
   donde `telefono` es el del cliente (con código de país, sin espacios) y `mensajeCodificado` es el mensaje ya reemplazado, codificado con `encodeURIComponent`. Abrir en nueva pestaña o ventana.
6. El cliente recibe el mensaje; al pulsar el enlace del PDF se abre la URL pública y se descarga el archivo (sin login).

**Validación:** Si el cliente no tiene teléfono, mostrar aviso y no abrir WhatsApp.

---

## 6. Ejemplos de request/response

### Listar plantillas
```http
GET /api/whatsapp-templates
Authorization: Bearer <token>
```
```json
[
  {
    "id": 1,
    "nombre": "Por Defecto",
    "mensaje": "Hola {NombreCliente}, tu factura {NumeroFactura} por {Monto}. Descarga: {EnlacePDF}",
    "activa": true,
    "predeterminada": true
  }
]
```

### Plantilla predeterminada
```http
GET /api/whatsapp-templates/default
Authorization: Bearer <token>
```
```json
{
  "id": 1,
  "nombre": "Por Defecto",
  "mensaje": "Hola {NombreCliente}, tu factura {NumeroFactura} por {Monto}. Descarga: {EnlacePDF}",
  "activa": true,
  "predeterminada": true
}
```

### URL del PDF (para {EnlacePDF})
```http
GET /api/invoices/INV-001/pdf-url
Authorization: Bearer <token>
```
```json
{
  "pdfUrl": "https://trippilot.cowib.es/api/public/invoices/INV-001/pdf"
}
```

### Crear plantilla
```http
POST /api/whatsapp-templates
Authorization: Bearer <token>
Content-Type: application/json

{
  "nombre": "Recordatorio pago",
  "mensaje": "Recordatorio: factura {NumeroFactura} por {Monto}. Descargar: {EnlacePDF}",
  "activa": true,
  "predeterminada": false
}
```
Respuesta **201** con el objeto plantilla (incluye `id`).

---

## 7. Resumen de tareas

| # | Tarea | Endpoints |
|---|--------|-----------|
| 1 | Pantalla listado/CRUD plantillas WhatsApp (solo Admin) | GET list, GET default, GET by id, POST, PUT, DELETE |
| 2 | Formulario crear/editar plantilla (nombre, mensaje, activa, predeterminada) | POST, PUT |
| 3 | Botón “Enviar por WhatsApp” en factura(s) | GET default, GET pdf-url, reemplazo de variables, abrir wa.me |
| 4 | Reemplazo de variables en el mensaje (incluido EnlacePDF desde pdf-url) | — |
| 5 | Validar teléfono del cliente antes de abrir WhatsApp | — |

Con esto el frontend puede implementar la gestión de plantillas y el envío de facturas por WhatsApp usando la API ya disponible en el backend.
