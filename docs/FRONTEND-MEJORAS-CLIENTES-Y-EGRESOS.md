# Mejoras recientes para el frontend

Documento de referencia con los cambios en la API que el frontend debe tener en cuenta.

---

## 1. Clientes: Cédula → Pasaporte

El campo **Cédula** se sustituyó por **Pasaporte** en el modelo de cliente.

### API de clientes

- **GET** `/api/clients` (y listado paginado): cada ítem incluye **`pasaporte`** en lugar de `cedula`.
- **POST** `/api/clients` (crear) y **PUT** `/api/clients/{id}` (actualizar): el body debe enviar **`pasaporte`** en lugar de `cedula`. Sigue siendo opcional.
- **GET** `/api/clients/{id}` y **GET** `/api/clients/{id}/history`: el objeto `client` tiene **`pasaporte`**.

### Formato

| Antes     | Ahora      |
|-----------|------------|
| `cedula`  | `pasaporte` |

Tipo: string opcional (ej. número de pasaporte). La búsqueda en listado de clientes también filtra por `pasaporte`.

### Cambios en el frontend

- Formularios de cliente (alta/edición): etiqueta y campo **“Pasaporte”** en lugar de “Cédula”.
- Tablas/listas de clientes: columna **“Pasaporte”** usando `pasaporte`.
- Historial o detalle de cliente: mostrar **“Pasaporte”** con el valor de `pasaporte`.
- Export Excel/PDF de clientes: la primera columna es **“Pasaporte”** (el backend ya la envía así).

---

## 2. Ventas: Nombre del cliente en la tabla

El listado de ventas incluye el **nombre del cliente** en cada ítem para poder mostrarlo en la tabla sin llamadas adicionales.

### Endpoint

**GET** `/api/sales` (con filtros opcionales: `clientId`, `status`, `paymentMethod`, `dateFrom`, `dateTo`, `page`, `pageSize`).

### Respuesta

Cada elemento de `items` incluye **`clientName`** (string o null):

```json
{
  "items": [
    {
      "id": 1,
      "clientId": 5,
      "clientName": "María López",
      "date": "2025-02-20T00:00:00Z",
      "product": "Tour París",
      "amount": 1500.00,
      "status": "Completado",
      "paymentMethod": "Transferencia"
    }
  ],
  "totalCount": 50,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3,
  "totalAmountInCordobas": 55200.00,
  "totalPendingInCordobas": 0
}
```

### Cambios en el frontend

- En la tabla/listado de ventas, añadir una columna **“Cliente”** y mostrar `clientName` de cada ítem.
- Los export Excel y PDF de ventas ya incluyen la columna “Cliente” con el nombre.

---

## 3. Egresos restan de los ingresos

El resumen del panel y los reportes incluyen **total de egresos** y **balance** (ingresos − egresos). Al agregar un egreso, el balance se actualiza automáticamente.

### Endpoints afectados

- **GET** `/api/dashboard/summary`
- **GET** `/api/reports/income-vs-expenses` (con `dateFrom` y `dateTo` opcionales)

### Campos en la respuesta

| Campo             | Tipo   | Descripción |
|-------------------|--------|-------------|
| **totalExpenses** | number | Suma de todos los egresos en el período (en córdobas en dashboard). |
| **balance**       | number | Ingresos totales menos egresos: `totalRevenue - totalExpenses` (o `totalIncome - totalExpenses` en reportes). |

Ejemplo (dashboard):

```json
{
  "totalRevenue": 887871.00,
  "incomeFromSales": 297315.00,
  "incomeFromReservations": 195066.00,
  "incomeFromInvoices": 395490.00,
  "totalExpenses": 45000.00,
  "balance": 842871.00,
  "clientsCount": 72,
  "reservationsCount": 100,
  "paidReservationsCount": 50
}
```

### Cambios en el frontend

- Mostrar en el panel: **Ingresos totales**, **Egresos totales** y **Balance (ingresos netos)** usando `totalRevenue`, `totalExpenses` y `balance`.
- Tras crear o editar un egreso, al refrescar el resumen, `totalExpenses` y `balance` deben reflejar que los egresos restan del balance.

---

## 4. Facturas: Fecha de viaje y fecha de retorno

Las facturas tienen dos campos opcionales: **fecha de viaje** y **fecha de retorno**, para que la cliente pueda indicarlos al crear o editar una factura.

### API de facturas

- **GET** `/api/invoices`, **GET** `/api/invoices/{id}`: cada factura incluye **`travelDate`** y **`returnDate`** (fecha en ISO o null).
- **POST** `/api/invoices` (crear) y **PUT** `/api/invoices/{id}` (actualizar): el body puede enviar **`travelDate`** y **`returnDate`** (opcionales, formato fecha `yyyy-MM-dd` o ISO).

Ejemplo de body al crear/actualizar factura:

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

### Dónde aparecen

- **PDF de la factura**: se muestran “Fecha de viaje” y “Fecha de retorno” cuando tienen valor.
- **Listados y reportes** (incl. historial del cliente y export Excel/PDF): incluyen columnas de fecha de viaje y fecha de retorno.

### Cambios en el frontend

- En el formulario de factura (crear/editar), añadir dos campos opcionales: **“Fecha de viaje”** y **“Fecha de retorno”**, enviando `travelDate` y `returnDate` en el body del POST/PUT.
- En tablas o detalle de facturas, mostrar estas fechas cuando existan (por ejemplo en formato corto dd/MM/yyyy).

---

## 5. Base de datos y migraciones

Para que el backend funcione correctamente con estas mejoras, hay que tener aplicadas las migraciones:

| Migración | Descripción |
|-----------|-------------|
| `RenameCedulaToPasaporteInClients` | Renombra la columna `Cedula` a `Pasaporte` en la tabla `Clients`. |
| `AddTravelAndReturnDateToInvoices` | Añade las columnas `TravelDate` y `ReturnDate` (opcionales) en la tabla `Invoices`. |

Comando típico (en la carpeta del backend):

```bash
dotnet ef database update
```

Si se parte de una base vacía, ejecutar las migraciones y, si se usa, el script de seed. En el seed, los clientes usan ya la columna **Pasaporte** y las facturas pueden usar **TravelDate** y **ReturnDate** cuando se añadan datos de prueba.

---

## Resumen rápido

| Mejora | Cambio para el frontend |
|--------|--------------------------|
| Clientes | Usar **`pasaporte`** en lugar de `cedula` en formularios, listas, detalle y export. |
| Ventas | Mostrar **`clientName`** en la tabla de ventas (viene en cada ítem de GET `/api/sales`). |
| Egresos / Balance | Mostrar **`totalExpenses`** y **`balance`** en el panel; al agregar egreso, el balance refleja que los egresos restan. |
| Facturas | Añadir campos opcionales **“Fecha de viaje”** y **“Fecha de retorno”** (`travelDate`, `returnDate`) en crear/editar y en listados cuando existan. |
