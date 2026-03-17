# Análisis: Facturas para imprimir (PDF)

Resumen de cómo está implementada la factura en PDF y qué incluye.

---

## 1. Dónde se genera el PDF

- **Servicio:** `Services/InvoicePdfService.cs` → `GeneratePdf(invoiceId)`
- **Biblioteca:** QuestPDF (A4, estilo ticket).

---

## 2. Endpoints para obtener el PDF

| Uso | Endpoint | Autenticación | Uso típico |
|-----|----------|---------------|------------|
| **Imprimir / ver en la app** | `GET /api/invoices/{id}/pdf` | Sí (Bearer) | Botón "Imprimir" o "Ver PDF" en la app. Se sirve `inline` para abrir en navegador. |
| **Enlace para cliente (ej. WhatsApp)** | `GET /api/public/invoices/{id}/pdf` | No | Link en plantilla WhatsApp (`{EnlacePDF}`). Descarga con nombre `Factura-INV-001.pdf`. |
| **URL del enlace** | `GET /api/invoices/{id}/pdf-url` | Sí | Devuelve `{ "pdfUrl": "https://.../api/public/invoices/INV-001/pdf" }` para copiar o usar en plantilla. |

---

## 3. Contenido del PDF (orden en pantalla) — Estilo ticket

El PDF es **tipo ticket** (como un comprobante de super): compacto, con la información clara y el logo de la agencia arriba. Mismo PDF para imprimir desde la app y para la URL pública de descarga.

| Bloque | Contenido |
|--------|-----------|
| **Cabecera** | **Logo** (Assets/logo.png) centrado arriba; debajo nombre de la agencia y título "FACTURA". Línea gris. |
| **Datos documento** | Nº factura | Fecha (dd/MM/yyyy) | Estado (Pagado/Pendiente/Vencida). Línea. |
| **Cliente** | Título "CLIENTE", nombre, teléfono (si hay), email (si hay). Línea. |
| **Concepto** | Título "CONCEPTO", texto del concepto o "-". Línea. |
| **Forma de pago** | Solo si viene informada (Cordobas/Dolares/Transferencia). Línea. |
| **Vencimiento** | Fecha de vencimiento (dd/MM/yyyy) o "-". Línea. |
| **Fecha de viaje / retorno** | Solo si al menos una tiene valor. "Fecha de viaje:" y "Fecha de retorno:" en dd/MM/yyyy. Línea. |
| **Total** | Fondo gris claro. "TOTAL" + monto con moneda (NIO o USD según forma de pago). Si es USD, debajo: "Equivalente: X.XX NIO". Línea final. |
| **Pie** | Texto: "Documento generado por TripPilot · dd/MM/yyyy HH:mm". |

---

## 4. Datos que salen en el PDF

- De **factura:** Id, Date, Status, Concept, PaymentMethod, DueDate, TravelDate, ReturnDate, Amount.
- De **cliente:** Name, Phone, Email. (No se muestra pasaporte en el PDF actual.)
- De **configuración:** CompanyName, Currency, ExchangeRate (para equivalente en córdobas si pago en USD).

Todo lo anterior se toma de la factura guardada en BD; no hay datos “en vivo” aparte de la hora en el pie.

---

## 5. Lo que ya está bien

- Formato A4, márgenes 40, tipografía legible.
- Incluye fecha de viaje y fecha de retorno cuando existen.
- Forma de pago y equivalente en córdobas cuando el pago es en dólares.
- Mismo PDF para imprimir (app) y para el enlace público (WhatsApp).
- Nombre de archivo claro: `Factura-INV-001.pdf`.

---

## 6. Posibles mejoras (opcional)

- **Pasaporte del cliente:** Si la cliente quiere ver el pasaporte en la factura impresa, se puede añadir una línea bajo el email del cliente (solo si `Client.Pasaporte` tiene valor).
- **Dirección / NIF de la agencia:** Si en el futuro se guardan en configuración, se podrían mostrar en cabecera o pie.
- **Logo:** Hoy solo texto (nombre de agencia). Si se añade logo en configuración, se podría poner en la cabecera.

---

## 7. Logo y assets

- **Logo:** Está en **`wwwroot/images/logo.png`** (convención de ASP.NET Core para estáticos). El PDF de la factura lo lee desde ahí y lo pone en la cabecera, con tamaño limitado (ancho máx. 200 pt, alto máx. 52 pt).
- **URL:** Con `UseStaticFiles()` activo, el frontend puede usar la imagen en **`/images/logo.png`** (p. ej. `<img src="/images/logo.png" />` o la URL completa de tu API).

## 8. Resumen

La factura para imprimir está implementada en **estilo ticket** (como comprobante de super): logo arriba, datos compactos, mismas rutas para la app y para la URL pública. No hace falta cambiar nada para que funcione; las mejoras anteriores son opcionales según necesidades del negocio.
