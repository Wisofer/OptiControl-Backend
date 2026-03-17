# API TripPilot – Guía para el Frontend

Documento de referencia para implementar el frontend (React) que consume el backend TripPilot (agencia de viajes).

---

## 1. Configuración general

| Concepto | Valor |
|----------|--------|
| **Base URL producción** | `https://trippilot.cowib.es` |
| **Base URL desarrollo** | `http://localhost:5229` (o el puerto que use el backend) |
| **Prefijo de rutas** | Todas bajo `/api/...` |
| **Autenticación** | **JWT.** El login devuelve un `token` en el body. El resto de la API espera la cabecera `Authorization: Bearer <token>`. |
| **CORS** | Orígenes permitidos: `http://localhost:5173`, `http://localhost:3000` (y el origen del front en producción si aplica). |
| **Moneda** | Córdobas (NIO). Los montos son `number`. |
| **Fechas** | Formato ISO o `yyyy-MM-dd` (ej: `2025-02-25`). |
| **Content-Type** | `application/json` para body en POST/PUT. |
| **Respuestas** | JSON. Códigos HTTP estándar (200, 201, 400, 401, 404). Sin redirect a login: ante token inválido o ausente el backend responde **401**. |

### Cómo enviar el token (fetch / axios)

- **fetch:** en cada petición a la API (excepto login):  
  `headers: { 'Authorization': 'Bearer ' + token, 'Content-Type': 'application/json' }`
- **axios:** interceptor o por petición:  
  `headers: { Authorization: 'Bearer ' + token }`  
  No hace falta `withCredentials` ni cookies.
- **Ante 401:** borrar el token (p. ej. de `localStorage` o estado) y redirigir al usuario a `/login`.

---

## 2. Autenticación

### 2.1 Login (público)

**POST** `/api/auth/login`

**Body:**
```json
{
  "nombreUsuario": "admin",
  "contrasena": "admin"
}
```

**Respuesta 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "usuario": "admin",
    "nombreCompleto": "Administrador del Sistema",
    "rol": "Administrador",
    "estado": "Activo"
  }
}
```

- **Guardar** `token` en memoria o `localStorage` (o el almacenamiento que use el frontend).
- En **todas las peticiones** a la API (excepto este login), enviar la cabecera:  
  `Authorization: Bearer <token>`.

**Respuesta 401:** `{ "error": "Usuario o contraseña incorrectos." }`  
**Respuesta 400:** `{ "error": "Usuario y contraseña son requeridos." }`

---

### 2.2 Usuario actual (protegido)

**GET** `/api/auth/me`

**Headers:** `Authorization: Bearer <token>`

**Respuesta 200:**
```json
{
  "id": 1,
  "usuario": "admin",
  "nombreCompleto": "Administrador del Sistema",
  "rol": "Administrador",
  "estado": "Activo"
}
```

**401:** No autenticado.

---

### 2.3 Logout (protegido)

**POST** `/api/auth/logout`

**Headers:** `Authorization: Bearer <token>`

**Respuesta 200:** `{ "success": true }`  

Con JWT no hay invalidación en servidor: el frontend debe **borrar el token** (localStorage/estado) y redirigir a `/login`.

---

## 3. Clientes (Clients)

### 3.1 Listar clientes

**GET** `/api/clients`  
**GET** `/api/clients?search=texto` (opcional: buscar por nombre, cédula, email, teléfono)

**Respuesta 200:** Array de clientes.
```json
[
  {
    "id": 1,
    "pasaporte": "001-250185-0001A",
    "name": "Carlos López",
    "email": "carlos@ejemplo.com",
    "phone": "505 8123 4567",
    "status": "Pendiente",
    "lastTrip": null
  }
]
```

**Status:** `"Pendiente"` | `"Viajó"`. **lastTrip:** `null` o fecha (string ISO).

---

### 3.2 Obtener un cliente

**GET** `/api/clients/{id}`

**Respuesta 200:** Objeto cliente (como arriba).  
**404:** No encontrado.

---

### 3.3 Historial del cliente (modal)

**GET** `/api/clients/{id}/history`

**Respuesta 200:**
```json
{
  "client": { "id": 1, "pasaporte": "...", "name": "...", "email": "...", "phone": "...", "status": "...", "lastTrip": null },
  "reservations": [
    { "id": 1, "destination": "París", "startDate": "2025-06-01", "endDate": "2025-06-10", "amount": 2500, "paymentStatus": "Pendiente" }
  ],
  "sales": [
    { "id": 1, "date": "2025-02-20", "product": "Paquete Roma", "amount": 800, "status": "Completado" }
  ],
  "invoices": [
    { "id": "INV-001", "date": "2025-02-25", "dueDate": "2025-03-25", "amount": 1500.5, "status": "Pendiente", "concept": "Paquete París" }
  ],
  "activity": [
    { "id": 1, "type": "client", "description": "Cliente creado: Carlos López", "time": "2026-02-25T11:49:00.719072-06:00", "entityId": null, "clientId": 1 }
  ]
}
```

Todo ordenado por fecha descendente donde aplique. Usar para el modal de historial del cliente.

---

### 3.4 Crear cliente

**POST** `/api/clients`

**Body:**
```json
{
  "pasaporte": "001-250185-0001A",
  "name": "Carlos López",
  "email": "carlos@ejemplo.com",
  "phone": "505 8123 4567"
}
```

**Requeridos:** `name`, `email`. **Opcionales:** `pasaporte`, `phone`.  
**Status** por defecto: `"Pendiente"`. **lastTrip** por defecto: `null`.

**Respuesta 201:** Cliente creado (objeto completo). **Location** apunta a `/api/clients/{id}`.  
**400:** Validación (ej. name o email vacíos).

---

### 3.5 Actualizar cliente

**PUT** `/api/clients/{id}`

**Body:** Objeto cliente (incluir `id` igual al de la URL). Pueden enviarse solo los campos a cambiar.
```json
{
  "id": 1,
  "pasaporte": "001-250185-0001A",
  "name": "Carlos López",
  "email": "carlos@ejemplo.com",
  "phone": "505 8123 4567",
  "status": "Viajó",
  "lastTrip": "2025-06-10"
}
```

**Respuesta 200:** Cliente actualizado. **404:** No encontrado.

---

### 3.6 Eliminar cliente

**DELETE** `/api/clients/{id}`

**Respuesta 204:** Sin contenido. **404:** No encontrado.

---

### 3.7 Exportar clientes (Excel / PDF)

**GET** `/api/clients/export/excel?search=...`  
**GET** `/api/clients/export/pdf?search=...`

Mismos query opcionales que el listado (`search`). Respuesta: archivo para descargar. **Excel:** `Clientes.xlsx`. **PDF:** listado formal con cabecera de empresa, tabla (Cédula, Nombre, Correo, Teléfono, Estado) con colores suaves.

---

## 4. Reservaciones (Reservations)

### 4.1 Listar reservaciones

**GET** `/api/reservations`  
**Query opcionales:** `clientId`, `paymentStatus`, `paymentMethod`, `dateFrom`, `dateTo` (fechas `yyyy-MM-dd`).

**Respuesta 200:** Array de reservaciones (cada una puede incluir `client` poblado).
```json
[
  {
    "id": 1,
    "clientId": 1,
    "destination": "París",
    "startDate": "2025-06-01",
    "endDate": "2025-06-10",
    "amount": 2500,
    "paymentStatus": "Pendiente",
    "paymentMethod": "Cordobas"
  }
]
```

**paymentStatus:** `"Pagado"` | `"Pendiente"` | `"Parcial"`.  
**paymentMethod** (opcional): `"Cordobas"` | `"Dolares"` | `"Transferencia"` | null. Ver **docs/FRONTEND-FORMA-DE-PAGO.md**.

---

### 4.2 Obtener una reservación

**GET** `/api/reservations/{id}`

**Respuesta 200:** Objeto reservación. **404:** No encontrada.

---

### 4.3 Crear reservación

**POST** `/api/reservations`

**Body:**
```json
{
  "clientId": 1,
  "destination": "París",
  "startDate": "2025-06-01",
  "endDate": "2025-06-10",
  "amount": 2500,
  "paymentStatus": "Pendiente",
  "paymentMethod": "Cordobas"
}
```

**Requeridos:** `clientId`, `destination`, `startDate`, `endDate`, `amount`.  
**paymentStatus** por defecto: `"Pendiente"`. **paymentMethod** opcional.

**Respuesta 201:** Reservación creada. **400:** Validación.

---

### 4.4 Actualizar reservación

**PUT** `/api/reservations/{id}`

**Body:** Objeto reservación con `id` igual al de la URL.

**Respuesta 200:** Reservación actualizada. **404:** No encontrada.

---

### 4.5 Eliminar reservación

**DELETE** `/api/reservations/{id}`

**Respuesta 204.** **404:** No encontrada.

---

### 4.6 Exportar reservaciones (Excel / PDF)

**GET** `/api/reservations/export/excel?clientId=...&paymentStatus=...&paymentMethod=...&dateFrom=...&dateTo=...`  
**GET** `/api/reservations/export/pdf?clientId=...&paymentStatus=...&paymentMethod=...&dateFrom=...&dateTo=...`

Mismos query opcionales que el listado. Respuesta: **Excel** `Reservaciones.xlsx`, **PDF** listado formal (Cliente, Destino, Inicio, Fin, Monto, Estado pago, **Forma de pago**).

---

## 5. Ventas (Sales)

### 5.1 Listar ventas

**GET** `/api/sales`  
**Query opcionales:** `clientId`, `status`, `paymentMethod`, `dateFrom`, `dateTo`.

**Respuesta 200:** Objeto con ítems y totales en córdobas (montos en USD convertidos con la tasa de configuración).
```json
{
  "items": [
    {
      "id": 1,
      "clientId": 1,
      "date": "2025-02-20",
      "product": "Paquete Roma",
      "amount": 800,
      "status": "Completado",
      "paymentMethod": "Dolares"
    }
  ],
  "totalAmountInCordobas": 29440.00,
  "totalPendingInCordobas": 0
}
```
- **items:** array de ventas. **totalAmountInCordobas:** suma en C$ (Dolares convertidos con tasa). **totalPendingInCordobas:** suma en C$ de ventas Pendiente. Ver **docs/FRONTEND-FORMA-DE-PAGO.md** y sección Totales en córdobas en NOVEDADES.

---

### 5.2 Obtener una venta

**GET** `/api/sales/{id}`

**Respuesta 200:** Objeto venta. **404:** No encontrada.

---

### 5.3 Crear venta

**POST** `/api/sales`

**Body:**
```json
{
  "clientId": 1,
  "date": "2025-02-20",
  "product": "Paquete Roma",
  "amount": 800,
  "status": "Completado",
  "paymentMethod": "Dolares"
}
```

**Requeridos:** `clientId`, `product`, `amount`. **date** por defecto: hoy. **status** por defecto: `"Completado"`. **paymentMethod** opcional.

**Respuesta 201:** Venta creada. **400:** Validación.

---

### 5.4 Actualizar venta

**PUT** `/api/sales/{id}`

**Body:** Objeto venta con `id` igual al de la URL.

**Respuesta 200.** **404:** No encontrada.

---

### 5.5 Eliminar venta

**DELETE** `/api/sales/{id}`

**Respuesta 204.** **404:** No encontrada.

---

### 5.6 Exportar ventas (Excel / PDF)

**GET** `/api/sales/export/excel?clientId=...&status=...&paymentMethod=...&dateFrom=...&dateTo=...`  
**GET** `/api/sales/export/pdf?clientId=...&status=...&paymentMethod=...&dateFrom=...&dateTo=...`

Mismos query opcionales que el listado. Respuesta: **Excel** `Ventas.xlsx`, **PDF** listado formal (Cliente, Fecha, Producto, Monto, Estado, **Forma de pago**).

---

## 6. Facturas (Invoices)

### 6.1 Listar facturas

**GET** `/api/invoices`  
**Query opcionales:** `clientId`, `status`, `paymentMethod`, `dateFrom`, `dateTo`.

**Respuesta 200:** Array de facturas.
```json
[
  {
    "id": "INV-001",
    "clientId": 1,
    "date": "2025-02-25",
    "dueDate": "2025-03-25",
    "amount": 1500.5,
    "status": "Pendiente",
    "concept": "Paquete París",
    "paymentMethod": "Transferencia"
  }
]
```

**status:** `"Pagado"` | `"Pendiente"` | `"Vencida"`. **paymentMethod** (opcional): ver **docs/FRONTEND-FORMA-DE-PAGO.md**.

---

### 6.2 Obtener una factura

**GET** `/api/invoices/{id}`  
`{id}` puede ser el código (ej: `INV-001`).

**Respuesta 200:** Objeto factura. **404:** No encontrada.

---

### 6.3 Obtener próximo código de factura

**GET** `/api/invoices/next-code`

**Respuesta 200:** `{ "code": "INV-001" }` (o el siguiente disponible).

Útil para mostrar “Próxima factura: INV-002” antes de crear.

---

### 6.4 Crear factura

**POST** `/api/invoices`

**Body:**
```json
{
  "clientId": 1,
  "date": "2025-02-25",
  "dueDate": "2025-03-25",
  "travelDate": "2025-04-01",
  "returnDate": "2025-04-15",
  "amount": 1500.5,
  "status": "Pendiente",
  "concept": "Paquete París",
  "paymentMethod": "Transferencia"
}
```

**Requeridos:** `clientId`, `amount`. **Opcionales:** `date`, `dueDate`, `travelDate`, `returnDate`, `status`, `concept`, **paymentMethod**.  
Si no se envía `id`, el backend asigna automáticamente el siguiente código (INV-001, INV-002, …).

**Respuesta 201:** Factura creada (incluye `id` generado). **400:** Validación.

---

### 6.5 Actualizar factura

**PUT** `/api/invoices/{id}`

**Body:** Objeto factura con `id` igual al de la URL.

**Respuesta 200.** **404:** No encontrada.

---

### 6.6 Eliminar factura

**DELETE** `/api/invoices/{id}`

**Respuesta 204.** **404:** No encontrada.

---

### 6.6.1 Exportar facturas (Excel / PDF)

**GET** `/api/invoices/export/excel?clientId=...&status=...&paymentMethod=...&dateFrom=...&dateTo=...`  
**GET** `/api/invoices/export/pdf?clientId=...&status=...&paymentMethod=...&dateFrom=...&dateTo=...`

Mismos query opcionales que el listado. Respuesta: **Excel** `Facturas.xlsx`, **PDF** listado formal (Nº Factura, Cliente, Fecha, Vencimiento, Monto, Estado, Concepto, **Forma de pago**) con cabecera de empresa y colores suaves.

---

### 6.7 URL del PDF (para plantilla WhatsApp – {EnlacePDF})

**GET** `/api/invoices/{id}/pdf-url`

**Requerido:** Usuario autenticado.

**Respuesta 200:**
```json
{
  "pdfUrl": "https://trippilot.cowib.es/api/public/invoices/INV-001/pdf"
}
```

Usar esta URL en la plantilla WhatsApp como valor de la variable `{EnlacePDF}`. Al abrirla (sin login) se descarga el PDF de la factura.

**404:** Factura no encontrada.

---

### 6.8 PDF para imprimir o descargar (misma factura que WhatsApp)

**GET** `/api/invoices/{id}/pdf`

Requiere **Authorization** (Bearer). Devuelve el mismo PDF de la factura (estilo ticket) que se envía por WhatsApp. Pensado para que el usuario, desde la pantalla de facturas, pueda **imprimir** o descargar la factura y entregarla en persona al cliente.

- **Content-Disposition:** `inline` — el PDF se abre en el navegador; el usuario puede imprimir (Ctrl+P) o guardar.
- **Content-Type:** `application/pdf`.

**Respuesta 200:** Archivo PDF. **404:** Factura no encontrada. **500:** Error al generar el PDF.

---

### 6.9 Descarga pública del PDF (sin autenticación)

**GET** `/api/public/invoices/{id}/pdf`

No requiere **Authorization**. Pensado para enlaces enviados por WhatsApp: al abrir la URL se descarga el PDF de la factura.

**Respuesta 200:** Archivo PDF con header `Content-Disposition: attachment; filename="Factura-{id}.pdf"`.

**404:** Factura no encontrada.

---

## 7. Actividad (Activity)

### 7.1 Listar actividad reciente

**GET** `/api/activity`  
**Query opcionales:** `limit` (default 20), `from` (datetime ISO).

**Respuesta 200:** Array de actividades.
```json
[
  {
    "id": 1,
    "type": "client",
    "description": "Cliente creado: Carlos López",
    "time": "2026-02-25T11:49:00.719072-06:00",
    "entityId": null,
    "clientId": 1
  }
]
```

**type:** `"reservation"` | `"invoice"` | `"payment"` | `"client"`.  
Orden: más reciente primero. Sirve para dashboard y notificaciones del navbar.

---

## 8. Egresos (Expenses)

### 8.1 Listar egresos

**GET** `/api/expenses`  
**Query opcionales:** `dateFrom`, `dateTo`, `category`.

**Respuesta 200:** Array de egresos.
```json
[
  {
    "id": 1,
    "date": "2025-02-20",
    "concept": "Pago proveedor",
    "amount": 500,
    "category": "Operativo"
  }
]
```

**category:** `"Operativo"` | `"Fijo"` | `"Marketing"`.

---

### 8.2 Obtener un egreso

**GET** `/api/expenses/{id}`

**Respuesta 200.** **404:** No encontrado.

---

### 8.3 Crear egreso

**POST** `/api/expenses`

**Body:**
```json
{
  "date": "2025-02-20",
  "concept": "Pago proveedor",
  "amount": 500,
  "category": "Operativo"
}
```

**Requeridos:** `concept`, `amount`. **Opcionales:** `date`, `category` (default `"Operativo"`).

**Respuesta 201.** **400:** Validación.

---

### 8.4 Actualizar egreso

**PUT** `/api/expenses/{id}`

**Body:** Objeto egreso con `id` igual al de la URL.

**Respuesta 200.** **404:** No encontrado.

---

### 8.5 Eliminar egreso

**DELETE** `/api/expenses/{id}`

**Respuesta 204.** **404:** No encontrado.

---

## 9. Caja (Daily cash register)

### 9.1 Listar caja por rango

**GET** `/api/caja`  
**Query opcionales:** `dateFrom`, `dateTo` (si no se envían, se usa último mes).

**Respuesta 200:** Array de registros de caja.
```json
[
  {
    "id": 1,
    "date": "2025-02-25",
    "opening": 1000,
    "sales": 2500,
    "expenses": 300,
    "closing": 3200
  }
]
```

---

### 9.2 Obtener caja de una fecha

**GET** `/api/caja/{date}`  
`{date}` en formato `yyyy-MM-dd` (ej: `2025-02-25`).

**Respuesta 200:** Objeto caja. **404:** No hay registro para esa fecha.

---

### 9.3 Crear o actualizar caja

**POST** `/api/caja`

**Body:**
```json
{
  "date": "2025-02-25",
  "opening": 1000,
  "sales": 2500,
  "expenses": 300,
  "closing": 3200
}
```

Si ya existe registro para esa fecha, se actualiza. Si no, se crea.

**Respuesta 200:** Objeto caja (creado o actualizado).

---

### 9.4 Actualizar caja por fecha

**PUT** `/api/caja/{date}`  
`{date}`: `yyyy-MM-dd`.

**Body:** Objeto caja (mismos campos). **date** puede coincidir con la URL.

**Respuesta 200.** **400:** Si la fecha no se puede parsear.

---

## 10. Configuración (Settings)

### 10.1 Obtener configuración

**GET** `/api/settings`

**Respuesta 200:** Objeto de configuración (un solo registro global).
```json
{
  "id": 1,
  "companyName": "Aventours",
  "email": "info@aventours.com",
  "phone": "505 1234 5678",
  "address": "Chinandega, Nicaragua",
  "currency": "NIO",
  "language": "es",
  "exchangeRate": 36.8,
  "theme": "light",
  "soundVolume": 80,
  "alertsReservacionesPendientes": true,
  "alertsFacturasVencidas": true,
  "alertsRecordatorios": true,
  "updatedAt": "2025-02-25T12:00:00Z"
}
```

Si no hay registro, el backend puede devolver valores por defecto (ej. companyName `"Aventours"`).

---

### 10.2 Actualizar configuración

**PUT** `/api/settings`

**Body:** Objeto de configuración (parcial o completo). Solo se actualizan los campos enviados.

**Respuesta 200:** Configuración actualizada.

### 10.3 Tipo de cambio (pantalla dedicada)

Para una pantalla solo de **tipo de cambio (C$ = 1 USD)**:

- **GET** `/api/exchange-rate` → `{ "exchangeRate": 36.8 }`
- **PUT** `/api/exchange-rate` → body `{ "exchangeRate": 36.8 }` (exchangeRate > 0)

Ver **docs/FRONTEND-CONFIGURACION-AGENCIA-Y-TIPO-CAMBIO.md** para detalle.

### 10.4 Datos de la agencia (formulario dedicado)

Para el formulario **Datos de la Agencia** (nombre, moneda, correo, teléfono, dirección, “Última actualización”):

- **GET** `/api/agency` → datos para el formulario (incluye `updatedAt`)
- **PUT** `/api/agency` → body con `companyName` (requerido), `email`, `phone`, `address`, `currency`

No modifican el tipo de cambio. Ver **docs/FRONTEND-CONFIGURACION-AGENCIA-Y-TIPO-CAMBIO.md** para detalle.

---

Todos los endpoints de esta sección requieren rol **Administrador**.

### 11.1 Listar usuarios

**GET** `/api/users`

**Respuesta 200:** Array de usuarios (sin contraseña).
```json
[
  {
    "id": 1,
    "usuario": "admin",
    "nombreCompleto": "Administrador del Sistema",
    "rol": "Administrador",
    "estado": "Activo"
  }
]
```

**estado:** `"Activo"` | `"Inactivo"`.

---

### 11.2 Obtener un usuario

**GET** `/api/users/{id}`

**Respuesta 200:** Objeto usuario. **404:** No encontrado.

---

### 11.3 Crear usuario

**POST** `/api/users`

**Body:**
```json
{
  "nombreUsuario": "maria",
  "contrasena": "Prueba123",
  "nombreCompleto": "María García",
  "rol": "Usuario"
}
```

**Requeridos:** `nombreUsuario`, `contrasena`. **Opcionales:** `nombreCompleto`, `rol` (default `"Usuario"`).  
**rol:** `"Administrador"` | `"Usuario"`.

**Respuesta 201:** Usuario creado (sin contraseña). **400:** Si el nombre de usuario ya existe o faltan campos.

---

### 11.4 Actualizar usuario

**PUT** `/api/users/{id}`

**Body:**
```json
{
  "nombreUsuario": "maria",
  "nombreCompleto": "María García",
  "rol": "Usuario",
  "estado": "Activo"
}
```

No se envía ni se devuelve la contraseña. Para cambiar contraseña haría falta un endpoint específico si se implementa.

**Respuesta 200.** **400:** Si el nombre de usuario ya existe (para otro id). **404:** No encontrado.

---

### 11.5 Eliminar usuario

**DELETE** `/api/users/{id}`

Solo administradores. No se puede eliminar el propio usuario.

**Respuesta 204:** Sin body (eliminación correcta).

**Errores:**
- **400:** `{ "error": "No puedes eliminar tu propio usuario." }` — el id de la URL es el del usuario autenticado.
- **403:** `{ "error": "..." }` — el token no pertenece a un administrador (o el mensaje que devuelva la política de autorización).
- **404:** `{ "error": "Usuario no encontrado." }` — el id no existe en base de datos.
- **401:** No autenticado o token inválido (igual que el resto de la API).

---

## 12. Plantillas WhatsApp (solo Administrador)

CRUD de plantillas de mensaje para enviar facturas por WhatsApp. Variables que el frontend reemplazará: `{NombreCliente}`, `{CodigoCliente}`, `{NumeroFactura}`, `{Monto}`, `{Mes}`, `{Categoria}`, `{Estado}`, `{FechaCreacion}`, `{EnlacePDF}`.

### 12.1 Listar plantillas

**GET** `/api/whatsapp-templates`  
**Query opcional:** `onlyActive=true` (solo plantillas activas).

**Respuesta 200:** Array de plantillas.
```json
[
  {
    "id": 1,
    "nombre": "Por Defecto",
    "mensaje": "Hola {NombreCliente}, adjunto factura {NumeroFactura} por {Monto}. Descarga: {EnlacePDF}",
    "activa": true,
    "predeterminada": true
  }
]
```

### 12.2 Obtener plantilla predeterminada

**GET** `/api/whatsapp-templates/default`

**Respuesta 200:** Objeto plantilla. **404:** No hay plantilla predeterminada.

### 12.3 Obtener una plantilla

**GET** `/api/whatsapp-templates/{id}`

**Respuesta 200.** **404:** No encontrada.

### 12.4 Crear plantilla

**POST** `/api/whatsapp-templates`

**Body:**
```json
{
  "nombre": "Recordatorio pago",
  "mensaje": "Recordatorio: factura {NumeroFactura} por {Monto}. {EnlacePDF}",
  "activa": true,
  "predeterminada": false
}
```

**Requeridos:** `nombre`, `mensaje`. **Opcionales:** `activa` (default true), `predeterminada` (default false). Si `predeterminada: true`, se desmarca la anterior.

**Respuesta 201.** **400:** Validación.

### 12.5 Actualizar plantilla

**PUT** `/api/whatsapp-templates/{id}`

**Body:** Objeto con `nombre`, `mensaje`, `activa`, `predeterminada` (todos opcionales). Misma lógica de “solo una predeterminada”.

**Respuesta 200.** **404:** No encontrada.

### 12.6 Eliminar plantilla

**DELETE** `/api/whatsapp-templates/{id}`

**Respuesta 204.** **404:** No encontrada. **400:** No se puede eliminar la única plantilla predeterminada (asignar otra antes).

---

## 13. Dashboard

### 12.1 Resumen

**GET** `/api/dashboard/summary`

**Respuesta 200:** Ingresos totales = ventas (Completado) + reservaciones (Pagado) + facturas (Pagado). Todos los importes en **córdobas** (los ítems en USD se convierten con la tasa de la agencia).
```json
{
  "totalRevenue": 15000.5,
  "incomeFromSales": 5000,
  "incomeFromReservations": 7000,
  "incomeFromInvoices": 3000.5,
  "clientsCount": 25,
  "reservationsCount": 40,
  "paidReservationsCount": 30
}
```

---

### 12.2 Actividad reciente (dashboard)

**GET** `/api/dashboard/recent-activity`

**Respuesta 200:** Array de las últimas 10 actividades (mismo formato que `/api/activity`).

---

### 12.3 Ingresos mensuales

**GET** `/api/dashboard/monthly-income`
**Query opcional:** `months` (default 12).

**Respuesta 200:** Array por mes. El monto de cada mes es la suma de ventas (Completado) + reservaciones (Pagado, por StartDate) + facturas (Pagado) de ese mes.
```json
[
  { "year": 2025, "month": 1, "monthName": "Jan", "amount": 4200 },
  { "year": 2025, "month": 2, "monthName": "Feb", "amount": 3800 }
]
```

---

### 12.4 Estado de reservaciones (gráfico circular)

**GET** `/api/dashboard/reservations-status`

**Respuesta 200:**
```json
{
  "total": 40,
  "paid": 30,
  "pending": 8,
  "partial": 2
}
```

---

## 14. Reportes (Reports) – Agencia de viajes

Solo **5 reportes esenciales**. Todos aceptan **query opcionales** `dateFrom` y `dateTo` (yyyy-MM-dd). Si no se envían, se usa el último año. Cada reporte tiene **exportación a Excel y PDF**.

### 14.1 Resumen financiero (ingresos, egresos, balance)

**GET** `/api/reports/income-vs-expenses?dateFrom=...&dateTo=...`

**GET** `/api/reports/income-vs-expenses?dateFrom=...&dateTo=...`

**Respuesta 200:** Los ingresos suman **ventas (completadas) + reservaciones (pagadas) + facturas (pagadas)**. Todos en **córdobas** (USD convertidos con la tasa).  
`{ "dateFrom": "...", "dateTo": "...", "totalIncome": 20000, "incomeFromSales": 5000, "incomeFromReservations": 10000, "incomeFromInvoices": 5000, "totalExpenses": 5000, "balance": 15000 }`

**Exportar:**  
**GET** `/api/reports/income-vs-expenses/export/excel?dateFrom=...&dateTo=...` → `Resumen-financiero.xlsx`  
**GET** `/api/reports/income-vs-expenses/export/pdf?dateFrom=...&dateTo=...` → `Resumen-financiero.pdf`

---

### 14.2 Listado de ventas con total

**GET** `/api/reports/sales?dateFrom=...&dateTo=...`

**Respuesta 200:** `{ "dateFrom": "...", "dateTo": "...", "items": [ ... ventas con clientName, paymentMethod ... ], "totalAmount": 15000 }` — **totalAmount** en córdobas.

**Exportar:**  
**GET** `/api/reports/sales/export/excel?dateFrom=...&dateTo=...` → `Reporte-ventas.xlsx`  
**GET** `/api/reports/sales/export/pdf?dateFrom=...&dateTo=...` → `Reporte-ventas.pdf`

---

### 14.3 Listado de facturas con total y estado

**GET** `/api/reports/invoices?dateFrom=...&dateTo=...`

**Respuesta 200:** `{ "dateFrom": "...", "dateTo": "...", "items": [ ... facturas con clientName, status, concept, paymentMethod ... ], "totalInvoiced": 12000 }` — **totalInvoiced** en córdobas.

**Exportar:**  
**GET** `/api/reports/invoices/export/excel?dateFrom=...&dateTo=...` → `Reporte-facturas.xlsx`  
**GET** `/api/reports/invoices/export/pdf?dateFrom=...&dateTo=...` → `Reporte-facturas.pdf`

---

### 14.4 Listado de reservaciones

**GET** `/api/reports/reservations?dateFrom=...&dateTo=...`

**Respuesta 200:** `{ "dateFrom": "...", "dateTo": "...", "items": [ ... reservaciones con clientName, destination, startDate, endDate, amount, paymentStatus, paymentMethod ... ], "totalAmountInCordobas": 50000 }` — **totalAmountInCordobas** en C$.

**Exportar:**  
**GET** `/api/reports/reservations/export/excel?dateFrom=...&dateTo=...` → `Reporte-reservaciones.xlsx`  
**GET** `/api/reports/reservations/export/pdf?dateFrom=...&dateTo=...` → `Reporte-reservaciones.pdf`

---

### 14.5 Listado de egresos

**GET** `/api/reports/expenses?dateFrom=...&dateTo=...`

**Respuesta 200:** `{ "dateFrom": "...", "dateTo": "...", "items": [ ... egresos ... ], "totalAmount": 5000 }`

**Exportar:**  
**GET** `/api/reports/expenses/export/excel?dateFrom=...&dateTo=...` → `Reporte-egresos.xlsx`  
**GET** `/api/reports/expenses/export/pdf?dateFrom=...&dateTo=...` → `Reporte-egresos.pdf`

---

## 15. Resumen de rutas por recurso

| Recurso      | GET (list/one)     | POST     | PUT        | DELETE     |
|-------------|--------------------|----------|------------|------------|
| Auth        | /api/auth/me       | /api/auth/login, logout | -          | -          |
| Clients     | /api/clients, /api/clients/{id}, /api/clients/{id}/history, /api/clients/export/excel, /api/clients/export/pdf | /api/clients | /api/clients/{id} | /api/clients/{id} |
| Reservations| /api/reservations, /api/reservations/{id}, /api/reservations/export/excel, /api/reservations/export/pdf | /api/reservations | /api/reservations/{id} | /api/reservations/{id} |
| Sales       | /api/sales, /api/sales/{id}, /api/sales/export/excel, /api/sales/export/pdf | /api/sales | /api/sales/{id} | /api/sales/{id} |
| Invoices    | /api/invoices, /api/invoices/{id}, /api/invoices/next-code, /api/invoices/{id}/pdf-url, /api/invoices/{id}/pdf, /api/invoices/export/excel, /api/invoices/export/pdf | /api/invoices | /api/invoices/{id} | /api/invoices/{id} |
| Activity    | /api/activity      | -        | -          | -          |
| Expenses    | /api/expenses, /api/expenses/{id} | /api/expenses | /api/expenses/{id} | /api/expenses/{id} |
| Caja        | /api/caja, /api/caja/{date} | /api/caja | /api/caja/{date} | -          |
| Settings    | /api/settings     | -        | /api/settings | -          |
| Users       | /api/users, /api/users/{id} | /api/users | /api/users/{id} | /api/users/{id} |
| WhatsApp templates | /api/whatsapp-templates, /api/whatsapp-templates/default, /api/whatsapp-templates/{id} | /api/whatsapp-templates | /api/whatsapp-templates/{id} | /api/whatsapp-templates/{id} |
| Public (sin auth) | /api/public/invoices/{id}/pdf | - | - | -          |
| Dashboard   | /api/dashboard/summary, recent-activity, monthly-income, reservations-status | - | - | -          |
| Reports     | /api/reports/income-vs-expenses, sales, invoices, reservations, expenses (+ /export/excel y /export/pdf para cada uno) | - | - | -          |

---

## 16. Errores típicos

- **400 Bad Request:** Body inválido o validación (ej. campos requeridos, tipos). Cuerpo puede ser `{ "error": "mensaje" }` o `{ "errors": { "Campo": ["mensaje"] } }`.
- **401 Unauthorized:** No hay cookie de sesión o sesión expirada. Redirigir a login.
- **403 Forbidden:** Sin permiso (ej. usuario no administrador en `/api/users`).
- **404 Not Found:** Recurso no encontrado.
- **500:** Error interno; no exponer detalles al usuario.

---

## 17. Usuario por defecto (solo para desarrollo/primer acceso)

- **Usuario:** `admin`  
- **Contraseña:** `admin`  
- **Rol:** Administrador  

Recomendación: cambiar la contraseña en producción y crear usuarios desde **Configuración → Gestión de usuarios** (POST `/api/users`).

---

---

## 18. Autenticación JWT – Resumen para el frontend

1. **Login:** `POST /api/auth/login` con `{ nombreUsuario, contrasena }` → respuesta con `token` y `user`.
2. **Guardar** el `token` (p. ej. en `localStorage` o estado global).
3. **Cada petición** a la API (salvo login): cabecera `Authorization: Bearer <token>`.
4. **Sin cookies:** no usar `credentials: 'include'`; el backend no usa cookies.
5. **Ante 401:** borrar token y redirigir a `/login`.
6. **Logout:** opcionalmente llamar `POST /api/auth/logout` con el token; en cualquier caso, borrar el token en el frontend.

Así se evitan problemas de CORS con cookies y redirects; el backend solo responde 401 cuando el token falta o es inválido.

*Documento generado para el frontend TripPilot (Aventours). Backend .NET; consumo desde React con JWT (Bearer token).*
