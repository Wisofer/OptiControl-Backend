-- TripPilot - Seed demo con datos REALES (~3 meses de uso)
-- Ejecutar sobre BD vacía. Incluye: usuarios reales, agencia, 70 clientes con nombres reales,
-- 100 reservaciones, 180 ventas, 150 facturas, 60 egresos, 250 actividades.
-- Para vaciar antes: TRUNCATE TABLE "Reservations", "Sales", "Invoices", "Clients", "Activities", "Expenses", "CajaDiaria", "WhatsAppTemplates", "AgencySettings", "Usuarios" RESTART IDENTITY CASCADE;

BEGIN;

-- ========== 1. Usuarios reales (admin/admin, resto usuario/usuario) ==========
-- Hash "admin" = jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=
-- Hash "usuario" = klDiIsTHHwxY1MVLUKiAoxLp+f7VXVw6oLDoYN7ZkWU=
INSERT INTO "Usuarios" ("NombreUsuario", "Contrasena", "Rol", "NombreCompleto", "Activo") VALUES
('admin', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Administrador', 'Administrador del Sistema', true),
('mlopez', 'klDiIsTHHwxY1MVLUKiAoxLp+f7VXVw6oLDoYN7ZkWU=', 'Usuario', 'María López García', true),
('cmartinez', 'klDiIsTHHwxY1MVLUKiAoxLp+f7VXVw6oLDoYN7ZkWU=', 'Usuario', 'Carlos Martínez Rodríguez', true),
('kgonzalez', 'klDiIsTHHwxY1MVLUKiAoxLp+f7VXVw6oLDoYN7ZkWU=', 'Administrador', 'Karen González Hernández', true),
('jperez', 'klDiIsTHHwxY1MVLUKiAoxLp+f7VXVw6oLDoYN7ZkWU=', 'Usuario', 'José Pérez Sánchez', true);

-- ========== 2. Configuración agencia ==========
INSERT INTO "AgencySettings" ("CompanyName", "Email", "Phone", "Address", "Currency", "Language", "ExchangeRate", "Theme", "SoundVolume", "AlertsReservacionesPendientes", "AlertsFacturasVencidas", "AlertsRecordatorios", "UpdatedAt")
VALUES ('Aventours', 'contacto@aventours.com', '505 8123 4567', 'De la Iglesia El Calvario 2c al sur, Managua', 'NIO', 'es', 36.8, 'light', 80, true, true, true, NOW());

-- ========== 2b. Plantillas WhatsApp (facturas) ==========
INSERT INTO "WhatsAppTemplates" ("Nombre", "Mensaje", "Activa", "Predeterminada") VALUES
('Factura con enlace PDF', 'Hola {NombreCliente}, su factura {NumeroFactura} por un monto de {Monto} está {Estado}. Puede descargarla aquí: {EnlacePDF}. Gracias por confiar en nosotros.', true, true),
('Recordatorio factura pendiente', 'Hola {NombreCliente}, le recordamos que la factura {NumeroFactura} ({Monto}) sigue pendiente de pago. Enlace: {EnlacePDF}.', true, false);

-- ========== 3. Clientes con nombres, pasaporte, correos y teléfonos reales (70) ==========
INSERT INTO "Clients" ("Pasaporte", "Name", "Email", "Phone", "Status", "LastTrip") VALUES
('001-280185-1023A', 'María López García', 'maria.lopez@gmail.com', '505 8765 4321', 'Viajó', '2025-12-15'::timestamp with time zone),
('001-150790-0987B', 'Carlos Martínez Rodríguez', 'carlos.martinez@yahoo.com', '505 8876 1234', 'Pendiente', NULL),
('001-220392-1122C', 'Ana Hernández Silva', 'ana.hernandez@gmail.com', '505 8987 6543', 'Viajó', '2026-01-08'::timestamp with time zone),
('001-100595-0756D', 'José Pérez González', 'jose.perez@outlook.com', '505 7654 3210', 'Pendiente', NULL),
('001-050888-1345E', 'Carmen Reyes Díaz', 'carmen.reyes@gmail.com', '505 8234 5678', 'Viajó', '2025-11-20'::timestamp with time zone),
('001-180291-0988F', 'Luis García Martínez', 'luis.garcia@hotmail.com', '505 8345 6789', 'Pendiente', NULL),
('001-250677-1156G', 'Patricia Sánchez López', 'patricia.sanchez@gmail.com', '505 8456 7890', 'Viajó', '2026-02-01'::timestamp with time zone),
('001-120393-0923H', 'Roberto Fernández Ruiz', 'roberto.fernandez@yahoo.com', '505 8567 8901', 'Pendiente', NULL),
('001-080694-1211I', 'Laura Martínez Hernández', 'laura.martinez@gmail.com', '505 8678 9012', 'Viajó', '2025-12-28'::timestamp with time zone),
('001-300186-1078J', 'Miguel Ángel Cruz', 'miguel.cruz@outlook.com', '505 8789 0123', 'Pendiente', NULL),
('001-140489-0845K', 'Sofía Ramírez Vega', 'sofia.ramirez@gmail.com', '505 8890 1234', 'Viajó', '2026-01-15'::timestamp with time zone),
('001-220592-1167L', 'Daniel Torres Méndez', 'daniel.torres@yahoo.com', '505 8901 2345', 'Pendiente', NULL),
('001-060791-0992M', 'Elena Morales Castro', 'elena.morales@gmail.com', '505 8012 3456', 'Viajó', '2025-11-10'::timestamp with time zone),
('001-170994-1024N', 'Andrés Gómez Flores', 'andres.gomez@hotmail.com', '505 8123 4567', 'Pendiente', NULL),
('001-280385-1135O', 'Isabel Vargas Ríos', 'isabel.vargas@gmail.com', '505 8234 5678', 'Viajó', '2026-02-10'::timestamp with time zone),
('001-090688-0867P', 'Francisco Jiménez Soto', 'francisco.jimenez@gmail.com', '505 8345 6789', 'Pendiente', NULL),
('001-240191-1189Q', 'Rosa María Ortiz', 'rosa.ortiz@yahoo.com', '505 8456 7890', 'Viajó', '2025-12-05'::timestamp with time zone),
('001-110492-0912R', 'Antonio Ruiz Mendoza', 'antonio.ruiz@outlook.com', '505 8567 8901', 'Pendiente', NULL),
('001-030795-1245S', 'Lucía Herrera Guzmán', 'lucia.herrera@gmail.com', '505 8678 9012', 'Viajó', '2026-01-22'::timestamp with time zone),
('001-190290-0878T', 'Pedro Castillo Luna', 'pedro.castillo@gmail.com', '505 8789 0123', 'Pendiente', NULL),
('001-260493-1096U', 'Mónica Delgado Serrano', 'monica.delgado@yahoo.com', '505 8890 1234', 'Viajó', '2025-11-28'::timestamp with time zone),
('001-070886-0823V', 'Javier Romero Navarro', 'javier.romero@gmail.com', '505 8901 2345', 'Pendiente', NULL),
('001-150194-1156W', 'Adriana Mendoza Acosta', 'adriana.mendoza@hotmail.com', '505 8012 3456', 'Viajó', '2026-02-05'::timestamp with time zone),
('001-210597-0945X', 'Fernando Silva Campos', 'fernando.silva@gmail.com', '505 8123 4567', 'Pendiente', NULL),
('001-040389-1212Y', 'Gabriela Ríos Peña', 'gabriela.rios@gmail.com', '505 8234 5678', 'Viajó', '2025-12-18'::timestamp with time zone),
('001-180692-0889Z', 'Ricardo Acosta Núñez', 'ricardo.acosta@yahoo.com', '505 8345 6789', 'Pendiente', NULL),
('001-290285-1123A', 'Natalia Flores León', 'natalia.flores@gmail.com', '505 8456 7890', 'Viajó', '2026-01-30'::timestamp with time zone),
('001-100488-0867B', 'Sergio León Cabrera', 'sergio.leon@outlook.com', '505 8567 8901', 'Pendiente', NULL),
('001-220791-1178C', 'Valentina Cabrera Domínguez', 'valentina.cabrera@gmail.com', '505 8678 9012', 'Viajó', '2025-11-15'::timestamp with time zone),
('001-060994-0912D', 'Alejandro Domínguez Reyes', 'alejandro.dominguez@gmail.com', '505 8789 0123', 'Pendiente', NULL),
('001-170387-1034E', 'Paula Reyes Moreno', 'paula.reyes@yahoo.com', '505 8890 1234', 'Viajó', '2026-02-12'::timestamp with time zone),
('001-250690-0856F', 'Martín Moreno Vega', 'martin.moreno@gmail.com', '505 8901 2345', 'Pendiente', NULL),
('001-080193-1189G', 'Claudia Vega Sandoval', 'claudia.vega@hotmail.com', '505 8012 3456', 'Viajó', '2025-12-22'::timestamp with time zone),
('001-140496-0923H', 'Raúl Sandoval Fuentes', 'raul.sandoval@gmail.com', '505 8123 4567', 'Pendiente', NULL),
('001-300189-1211I', 'Diana Fuentes Orellana', 'diana.fuentes@gmail.com', '505 8234 5678', 'Viajó', '2026-01-05'::timestamp with time zone),
('001-110592-0878J', 'Héctor Orellana Ponce', 'hector.orellana@yahoo.com', '505 8345 6789', 'Pendiente', NULL),
('001-200795-1096K', 'Mariana Ponce Salazar', 'mariana.ponce@gmail.com', '505 8456 7890', 'Viajó', '2025-11-25'::timestamp with time zone),
('001-050388-0823L', 'Oscar Salazar Rojas', 'oscar.salazar@outlook.com', '505 8567 8901', 'Pendiente', NULL),
('001-160691-1156M', 'Andrea Rojas Espinoza', 'andrea.rojas@gmail.com', '505 8678 9012', 'Viajó', '2026-02-18'::timestamp with time zone),
('001-240294-0945N', 'Pablo Espinoza Miranda', 'pablo.espinoza@gmail.com', '505 8789 0123', 'Pendiente', NULL),
('001-090587-1212O', 'Camila Miranda Solís', 'camila.miranda@yahoo.com', '505 8890 1234', 'Viajó', '2025-12-08'::timestamp with time zone),
('001-270890-0889P', 'Lorenzo Solís Córdoba', 'lorenzo.solis@gmail.com', '505 8901 2345', 'Pendiente', NULL),
('001-120193-1123Q', 'Victoria Córdoba Paredes', 'victoria.cordoba@gmail.com', '505 8012 3456', 'Viajó', '2026-01-18'::timestamp with time zone),
('001-030496-0867R', 'Emilio Paredes Lara', 'emilio.paredes@hotmail.com', '505 8123 4567', 'Pendiente', NULL),
('001-180789-1178S', 'Renata Lara Montes', 'renata.lara@gmail.com', '505 8234 5678', 'Viajó', '2025-11-30'::timestamp with time zone),
('001-280392-0912T', 'Gustavo Montes Carrillo', 'gustavo.montes@yahoo.com', '505 8345 6789', 'Pendiente', NULL),
('001-070695-1034U', 'Beatriz Carrillo Sosa', 'beatriz.carrillo@gmail.com', '505 8456 7890', 'Viajó', '2026-02-22'::timestamp with time zone),
('001-150198-0856V', 'Diego Sosa Maldonado', 'diego.sosa@gmail.com', '505 8567 8901', 'Pendiente', NULL),
('001-230491-1189W', 'Elena Maldonado Aguilar', 'elena.maldonado@outlook.com', '505 8678 9012', 'Viajó', '2025-12-12'::timestamp with time zone),
('001-100794-0923X', 'Felipe Aguilar Figueroa', 'felipe.aguilar@gmail.com', '505 8789 0123', 'Pendiente', NULL),
('001-260387-1211Y', 'Lorena Figueroa Mejía', 'lorena.figueroa@gmail.com', '505 8890 1234', 'Viajó', '2026-01-25'::timestamp with time zone),
('001-040690-0878Z', 'Ignacio Mejía Contreras', 'ignacio.mejia@yahoo.com', '505 8901 2345', 'Pendiente', NULL),
('001-190293-1096A', 'Regina Contreras Valdez', 'regina.contreras@gmail.com', '505 8012 3456', 'Viajó', '2025-11-18'::timestamp with time zone),
('001-080586-0823B', 'Arturo Valdez Cervantes', 'arturo.valdez@gmail.com', '505 8123 4567', 'Pendiente', NULL),
('001-210889-1156C', 'Alicia Cervantes Márquez', 'alicia.cervantes@hotmail.com', '505 8234 5678', 'Viajó', '2026-02-08'::timestamp with time zone),
('001-130192-0945D', 'Bruno Márquez Guzmán', 'bruno.marquez@gmail.com', '505 8345 6789', 'Pendiente', NULL),
('001-300495-1212E', 'Cecilia Guzmán Ibarra', 'cecilia.guzman@yahoo.com', '505 8456 7890', 'Viajó', '2025-12-25'::timestamp with time zone),
('001-110788-0889F', 'Eduardo Ibarra Téllez', 'eduardo.ibarra@gmail.com', '505 8567 8901', 'Pendiente', NULL),
('001-240191-1123G', 'Florencia Téllez Campos', 'florencia.tellez@gmail.com', '505 8678 9012', 'Viajó', '2026-01-12'::timestamp with time zone),
('001-050494-0867H', 'Gerardo Campos Escobar', 'gerardo.campos@outlook.com', '505 8789 0123', 'Pendiente', NULL),
('001-160797-1178I', 'Helena Escobar Nava', 'helena.escobar@gmail.com', '505 8890 1234', 'Viajó', '2025-11-22'::timestamp with time zone),
('001-270390-0912J', 'Iván Nava Quintero', 'ivan.nava@gmail.com', '505 8901 2345', 'Pendiente', NULL),
('001-090693-1034K', 'Julia Quintero Zamora', 'julia.quintero@yahoo.com', '505 8012 3456', 'Viajó', '2026-02-15'::timestamp with time zone),
('001-200186-0856L', 'Kevin Zamora Barrios', 'kevin.zamora@gmail.com', '505 8123 4567', 'Pendiente', NULL),
('001-010489-1189M', 'Leticia Barrios Gallegos', 'leticia.barrios@gmail.com', '505 8234 5678', 'Viajó', '2025-12-02'::timestamp with time zone),
('001-170792-0923N', 'Mauricio Gallegos Durán', 'mauricio.gallegos@hotmail.com', '505 8345 6789', 'Pendiente', NULL),
('001-280095-1211O', 'Noemí Durán Benítez', 'noemi.duran@gmail.com', '505 8456 7890', 'Viajó', '2026-01-28'::timestamp with time zone),
('001-120398-0878P', 'Óscar Benítez Cuevas', 'oscar.benitez@yahoo.com', '505 8567 8901', 'Pendiente', NULL),
('001-230591-1096Q', 'Paulina Cuevas Duarte', 'paulina.cuevas@gmail.com', '505 8678 9012', 'Viajó', '2025-11-08'::timestamp with time zone),
('001-060894-0823R', 'Quique Duarte Fajardo', 'quique.duarte@gmail.com', '505 8789 0123', 'Pendiente', NULL),
('001-150197-1156S', 'Rocío Fajardo Galindo', 'rocio.fajardo@outlook.com', '505 8890 1234', 'Viajó', '2026-02-25'::timestamp with time zone),
('001-250490-0945T', 'Salvador Galindo Haro', 'salvador.galindo@gmail.com', '505 8901 2345', 'Pendiente', NULL);

-- ========== 4. Reservaciones reales (100) ==========
INSERT INTO "Reservations" ("ClientId", "Destination", "StartDate", "EndDate", "Amount", "PaymentStatus", "PaymentMethod")
SELECT
  1 + (n % 70),
  (ARRAY['San Juan del Sur', 'Corn Island', 'Isla de Ometepe', 'León', 'Granada', 'Masaya', 'Managua', 'Rivas', 'Estelí', 'Matagalpa', 'Bluefields', 'Tola', 'Popoyo', 'Las Peñitas', 'Solentiname', 'Río San Juan', 'El Castillo', 'San Carlos'])[1 + (n % 18)],
  ('2025-11-27'::date + (n * 3 % 92))::timestamp with time zone,
  ('2025-11-27'::date + (n * 3 % 92) + 2 + (n % 5))::timestamp with time zone,
  (ARRAY[125.00, 350.00, 280.00, 95.00, 420.00, 180.00, 550.00, 210.00, 380.00, 165.00, 490.00, 220.00, 310.00, 145.00, 275.00, 195.00, 440.00, 260.00])[1 + (n % 18)],
  (ARRAY['Pagado', 'Pagado', 'Pendiente', 'Parcial'])[1 + (n % 4)],
  (ARRAY['Cordobas', 'Dolares', 'Transferencia', 'TransferenciaDolares'])[1 + (n % 4)]
FROM generate_series(1, 100) AS n;

-- ========== 5. Ventas reales (180) ==========
INSERT INTO "Sales" ("ClientId", "Date", "Product", "Amount", "Status", "PaymentMethod")
SELECT
  1 + (n % 70),
  ('2025-11-27'::date + (n * 2 % 92))::timestamp with time zone,
  (ARRAY['Tour colonial Granada', 'Paquete 3 días Ometepe', 'Traslado aeropuerto Managua', 'Tour volcán Masaya', 'Paquete Corn Island 4 noches', 'Tour San Juan del Sur y playas', 'Tour León y ruinas', 'Tour Rivas y frontera', 'Combo Granada + Masaya', 'Tour islas Solentiname', 'Paquete café Matagalpa', 'Tour Estelí y cigarros', 'Traslado hotel-aeropuerto', 'Paquete Tola 2 noches', 'Tour surf Popoyo', 'Tour Río San Juan', 'Paquete Bluefields', 'Traslado privado Granada-Managua', 'Tour día completo Masaya', 'Entrada volcán Mombacho'])[1 + (n % 20)],
  (ARRAY[45.00, 285.00, 35.00, 28.00, 420.00, 95.00, 55.00, 40.00, 120.00, 180.00, 195.00, 75.00, 30.00, 250.00, 65.00, 320.00, 380.00, 50.00, 42.00, 22.00])[1 + (n % 20)],
  (ARRAY['Completado', 'Completado', 'Completado', 'Pendiente'])[1 + (n % 4)],
  (ARRAY['Cordobas', 'Dolares', 'Transferencia', 'TransferenciaDolares'])[1 + (n % 4)]
FROM generate_series(1, 180) AS n;

-- ========== 6. Facturas reales (150) ==========
INSERT INTO "Invoices" ("Id", "ClientId", "Date", "DueDate", "Amount", "Status", "Concept", "PaymentMethod")
SELECT
  'INV-' || lpad(n::text, 3, '0'),
  1 + (n % 70),
  ('2025-11-27'::date + ((n * 5) % 92))::timestamp with time zone,
  ('2025-11-27'::date + ((n * 5) % 92) + 15)::timestamp with time zone,
  (ARRAY[150.00, 320.00, 85.00, 450.00, 220.00, 180.00, 520.00, 95.00, 380.00, 275.00, 410.00, 165.00, 290.00, 195.00, 340.00])[1 + (n % 15)],
  (ARRAY['Pagado', 'Pagado', 'Pendiente', 'Vencida'])[1 + (n % 4)],
  (ARRAY['Paquete turístico Granada y Masaya', 'Reserva tour Ometepe 3 días', 'Traslados aeropuerto', 'Paquete Corn Island 5 noches', 'Tour volcán Masaya y laguna', 'Servicio guía San Juan del Sur', 'Paquete León y playas', 'Entradas y traslado Mombacho', 'Tour Solentiname 2 días', 'Reserva hotel + tour Matagalpa', 'Paquete Rivas y surf', 'Traslado privado Managua-Granada', 'Tour Estelí y fábrica cigarros', 'Servicio transporte grupo', 'Paquete Bluefields 3 noches'])[1 + (n % 15)],
  (ARRAY['Cordobas', 'Dolares', 'Transferencia', 'TransferenciaDolares'])[1 + (n % 4)]
FROM generate_series(1, 150) AS n;

-- ========== 7. Egresos reales (60) ==========
INSERT INTO "Expenses" ("Date", "Concept", "Amount", "Category")
SELECT
  ('2025-11-27'::date + (n * 4 % 92))::timestamp with time zone,
  (ARRAY['Comisión BAC 1.5% venta tarjeta', 'Combustible Toyota Hilux', 'Papel bond y tintas oficina', 'Facebook Ads campaña Semana Santa', 'Cambio aceite y filtros vehículo', 'Alquiler local oficina diciembre', 'Energía y agua noviembre', 'Comisión agente María López', 'Impresión folletos 500 unidades', 'Refrigerio reunión equipo', 'Peaje carretera Managua-Masaya', 'Mantenimiento aire acondicionado', 'Dominio trippilot.com.ni', 'Comisión agente Carlos Martínez', 'Recarga datos móvil oficina', 'Lavado y aspirado vehículo', 'Café y agua para clientes', 'Seguro responsabilidad civil', 'Comisión transferencia bancaria', 'Material señalética oficina'])[1 + (n % 20)],
  (ARRAY[12.50, 85.00, 45.00, 120.00, 65.00, 350.00, 78.00, 42.00, 28.00, 15.00, 8.00, 95.00, 25.00, 38.00, 12.00, 18.00, 22.00, 180.00, 5.50, 55.00])[1 + (n % 20)],
  (ARRAY['Operativo', 'Operativo', 'Operativo', 'Marketing', 'Operativo', 'Fijo', 'Fijo', 'Operativo', 'Marketing', 'Operativo', 'Operativo', 'Operativo', 'Fijo', 'Operativo', 'Operativo', 'Operativo', 'Operativo', 'Fijo', 'Operativo', 'Operativo'])[1 + (n % 20)]
FROM generate_series(1, 60) AS n;

-- ========== 8. Actividades reales (250) ==========
INSERT INTO "Activities" ("Type", "Description", "Time", "EntityId", "ClientId")
SELECT
  (ARRAY['reservation', 'invoice', 'payment', 'client'])[1 + (n % 4)],
  CASE (n % 4)
    WHEN 0 THEN 'Reservación creada: ' || (ARRAY['San Juan del Sur', 'Ometepe', 'Corn Island', 'Granada', 'Masaya'])[1 + (n % 5)] || ' para cliente #' || (1 + (n % 70))
    WHEN 1 THEN 'Factura INV-' || lpad((1 + (n % 150))::text, 3, '0') || ' emitida'
    WHEN 2 THEN 'Pago registrado - reservación #' || (1 + (n % 100))
    ELSE 'Cliente registrado en sistema'
  END,
  ('2025-11-27'::date + (n % 92))::timestamp with time zone,
  CASE (n % 4)
    WHEN 0 THEN 'RES-' || (1 + (n % 100))
    WHEN 1 THEN 'INV-' || lpad((1 + (n % 150))::text, 3, '0')
    WHEN 2 THEN 'PAY-' || n
    ELSE NULL
  END,
  1 + (n % 70)
FROM generate_series(1, 250) AS n;

COMMIT;
