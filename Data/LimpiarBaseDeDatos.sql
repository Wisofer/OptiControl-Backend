-- Limpia toda la base de datos y deja solo 1 usuario (admin) y configuración de agencia
-- Para poder seguir usando login y que el dashboard funcione.

BEGIN;

TRUNCATE TABLE "Activities", "Invoices", "Reservations", "Sales", "Clients", "Expenses", "CajaDiaria", "WhatsAppTemplates", "AgencySettings", "Usuarios" RESTART IDENTITY CASCADE;

-- Usuario admin (contraseña: admin). Hash del seed.
INSERT INTO "Usuarios" ("NombreUsuario", "Contrasena", "Rol", "NombreCompleto", "Activo") VALUES
('admin', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Administrador', 'Administrador del Sistema', true);

-- Configuración mínima de agencia (tasa para conversión)
INSERT INTO "AgencySettings" ("CompanyName", "Email", "Phone", "Address", "Currency", "Language", "ExchangeRate", "Theme", "SoundVolume", "AlertsReservacionesPendientes", "AlertsFacturasVencidas", "AlertsRecordatorios", "UpdatedAt")
VALUES ('Aventours', 'contacto@aventours.com', '505 8123 4567', 'Managua', 'NIO', 'es', 36.8, 'light', 80, true, true, true, NOW());

COMMIT;
