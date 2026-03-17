-- Seed: 6 testimonios para "Lo que dicen nuestros clientes" (Rating 5 estrellas).
-- Ejecutar manualmente si lo necesitas: psql -U postgres -d trippilot -f Data/SeedTestimonials.sql

DELETE FROM "Testimonials";

INSERT INTO "Testimonials" ("Quote", "AuthorName", "Location", "Rating", "SortOrder", "IsActive", "IsApproved", "CreatedAt", "UpdatedAt") VALUES
('Aventours me ayudó a organizar mi viaje a Costa Rica. El equipo fue atento, profesional y creó un itinerario perfecto. Sin ellos, no hubiera sido posible.', 'Maria Rodríguez', 'Chinandega, Nicaragua', 5, 1, true, true, NOW(), NOW()),
('Excelente servicio para mi luna de miel. La reservación de vuelos y hoteles fue sin problemas. Recomiendo Aventours a todos mis amigos.', 'Juan López', 'León, Nicaragua', 5, 2, true, true, NOW(), NOW()),
('Viajé con mi familia a Panamá y todo fue impecable. Desde el inicio hasta el final, Aventours cuidó cada detalle de nuestro viaje.', 'Carmen Sánchez', 'Managua, Nicaragua', 5, 3, true, true, NOW(), NOW()),
('La asesoría para obtener mi visa fue valiosísima. El equipo de Aventours tiene experiencia real y compartió consejos muy útiles.', 'Roberto Gutiérrez', 'Masaya, Nicaragua', 5, 4, true, true, NOW(), NOW()),
('Tour nacional con Aventours fue increíble. Las playas, la atención del guía, todo fue perfecto. Muy buen precio también.', 'Sofia Mendoza', 'Granada, Nicaragua', 5, 5, true, true, NOW(), NOW()),
('Viajé a Miami y obtuve una oferta excelente. Aventours siempre tiene las mejores tarifas. Definitivamente vuelvo a usar sus servicios.', 'Diego Morales', 'Chinandega, Nicaragua', 5, 6, true, true, NOW(), NOW());
