-- Seed: 6 servicios para la sección "Nuestros Servicios" de la web.
-- Ejecutar manualmente si lo necesitas: psql -U postgres -d trippilot -f Data/SeedWebsiteServices.sql

DELETE FROM "WebsiteServices";

INSERT INTO "WebsiteServices" ("Title", "ShortDescription", "Description", "SortOrder", "IsActive", "Icon", "CreatedAt", "UpdatedAt") VALUES
('Reservación de Vuelos', 'Acceso a las mejores tarifas aéreas nacionales e internacionales.', 'Acceso a las mejores tarifas aéreas nacionales e internacionales. Comparamos opciones para encontrar el mejor precio.', 1, true, 'airplane', NOW(), NOW()),
('Paquetes Turísticos', 'Todo incluido: vuelos, hoteles, tours y transporte.', 'Todo incluido: vuelos, hoteles, tours y transporte. Experiencias diseñadas para cada tipo de viajero.', 2, true, 'palm-tree', NOW(), NOW()),
('Reservas de Hoteles', 'Alojamientos de todas las categorías.', 'Alojamientos de todas las categorías. Desde lujo hasta presupuesto, tenemos opciones para ti.', 3, true, 'hotel', NOW(), NOW()),
('Viajes Internacionales', 'Explora destinos internacionales con seguridad.', 'Explora destinos internacionales con seguridad. Incluye asesoría completa para tu viaje al extranjero.', 4, true, 'globe', NOW(), NOW()),
('Tours Nacionales', 'Descubre lo mejor de Nicaragua.', 'Descubre lo mejor de Nicaragua. Tours a destinos nacionales con guías especializados y transporte incluido.', 5, true, 'bus', NOW(), NOW()),
('Asesoría de Documentación', 'Asistencia completa con visas, pasaportes y documentación de viaje.', 'Asistencia completa con visas, pasaportes y documentación de viaje requerida por cada país.', 6, true, 'document', NOW(), NOW());
