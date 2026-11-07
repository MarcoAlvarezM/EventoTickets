/* =============================================================================
   M2 · Datos iniciales: eventos y tipos de entrada
   Módulo: Módulo 2 · Eventos, cartelera, tipos de entrada y disponibilidad (HU-04, 05, 06, 09, 10, 11)
   Las fechas son relativas a la fecha de instalación (siempre futuras).
   Depende de: M1_02_datos_iniciales.sql
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO
SET NOCOUNT ON;
GO

/* ---------- Eventos ---------- */
DECLARE @Hoy DATE = CAST(GETDATE() AS DATE);
INSERT INTO dbo.Eventos (CodigoReferencia, CategoriaId, LocacionId, Nombre, DescripcionCorta, DescripcionCompleta, Organizador, ImagenUrl, FechaEvento, HoraEvento, Estado, FechaCreacion)
VALUES
 ('EVT-TEC-2026-001', 1, 1, 'Cumbre Latinoamericana de Tecnología e IA',
  'Dos días de conferencias, talleres y networking sobre inteligencia artificial aplicada.',
  'La cumbre reúne a investigadores, desarrolladores y líderes de producto de la región. Incluye charlas magistrales, talleres prácticos de IA aplicada, zona de exhibición de startups y espacios de networking.',
  'Alianza Tech Centroamérica', 'https://lh3.googleusercontent.com/aida-public/AB6AXuDO87WVwwpGO7K_973jS-FMu5ON7pZeyl06vbwMV3TkZjhFcNPO5TghL0GThxqz1fHfCqo-ydtbI-C_KDjD3ClmADFyj1uUfC_TYrKfnajdforTcJxxvNduJGKO0ksuIBHs-6SnTFL2nBzlD8RMVrz5trqKmP08cbZ9ayaBJTgq_t5jWEOco3QNH4rBwjRfMCNOZS20RcaBvtt1TiNpGVOT0apb9t3mjZYWOV7aRMmoMNKBXWHrNffANw', DATEADD(DAY, 25, @Hoy), '09:00', 'Publicado', DATEADD(DAY,-40,GETDATE())),
 ('EVT-FES-2026-002', 2, 2, 'Festival de Jazz & Blues al Parque',
  'Una tarde de jazz y blues al aire libre con artistas nacionales e internacionales.',
  'Festival familiar al aire libre con tres escenarios, food trucks y zona de picnic. Entrada general y palcos especiales con vista preferencial al escenario principal.',
  'Producciones Sabana Viva', 'https://lh3.googleusercontent.com/aida-public/AB6AXuAJM63Ydf9NAqtiUo3wh_fF-gNUU9bmd1SEjxQkvji-m8M8teehKyuKq84zdgHHYxh6eGaz657TTmxwgDH2sjFyejt16V7j61al9zRCoEJVnm_RZgcNVjwfgLEu_j4cI1UhnePGeb4fsqsYKmoJ7Qp420JOty-7n8pi5lQKP2bUysmhqDMdm702_qJCovs13KVu1Yx0C9Yx6TApNSDu3oUx3phXZDrRZ38oTMQmkMyzcpwk_mO0pLlL_w', DATEADD(DAY, 32, @Hoy), '14:00', 'Publicado', DATEADD(DAY,-35,GETDATE())),
 ('EVT-CON-2026-003', 3, 3, 'Sinfonía Urbana & Filarmónica en Concierto',
  'La orquesta filarmónica interpreta clásicos del pop y del rock en formato sinfónico.',
  'Un recorrido por grandes éxitos contemporáneos con arreglos para orquesta completa, coro y solistas invitados en el escenario histórico del Teatro Nacional.',
  'Orquesta Filarmónica de Costa Rica', 'https://lh3.googleusercontent.com/aida-public/AB6AXuBdpayF2Y6uthAKH76SSwg-EZg9ZD9nyuA7kxB56va1WEOfpaxM150JpzREHbq_L1g7DneCxW5sErjWuKZcQYdl53KEOgctm-0w5fUYgNI2ThvtminehgSIRtXns9D-EOl73PYCcswi_Q3eewv4twyMb2cJI4BOYhJ915F9f43fEeUfd4ZaI22OBoiK30Y9To0vGGRxJmD96ZyU7RUb3NZVzRUhNK4sHqZrxUIWPkJtZmAkf3TXqBbrVw', DATEADD(DAY, 40, @Hoy), '20:00', 'Publicado', DATEADD(DAY,-30,GETDATE())),
 ('EVT-NEG-2026-004', 4, 4, 'Foro de Emprendimiento y Finanzas',
  'Conferencias y mentorías para emprendedores, inversionistas y equipos de finanzas.',
  'Jornada completa con paneles de inversión, rondas de mentoría uno a uno y espacio de networking con café incluido.',
  'Cámara de Emprendedores', 'https://lh3.googleusercontent.com/aida-public/AB6AXuByZrgPug6nqG27emHYH0Xrar3EYln-VMzgLVvXoRWNHUz0m9Q2mkhPwiWx5mbALMuFYLZ3ZG2Mp5fkPmY-CWdjSBx9YnGHDv5Af7mLP8_LDJIQs0W5IMqJad2s2DtCK_LqnAEk7pt3U_MKiBFhncVDNeIeIJYbSPysn-M12bGArvn-0r4qVS-1JpoGh5Gf83UIej5mMe7LzNOE6dxZf8dzZO8wZrW_vAqMAYUyyf3geYAKK8fNJ37UzA', DATEADD(DAY, 18, @Hoy), '08:30', 'Publicado', DATEADD(DAY,-25,GETDATE())),
 ('EVT-TEA-2026-005', 5, 3, 'Obra Teatral Clásica: Noche de Dramaturgia',
  'Puesta en escena de un clásico de la dramaturgia universal con elenco nacional.',
  'Montaje de gran formato con escenografía original y música en vivo. Función con elenco estelar y conversatorio posterior con el director.',
  'Compañía Nacional de Teatro', 'https://lh3.googleusercontent.com/aida-public/AB6AXuAg8wLc_7_Ot9kYe_OBnMXF65Qhr2juv11bTLGeShuL-rzhpZw3Ej753qZ_fFrcedeFs648XW090P4xXPjqLsktYLZPdlWBoRH6g9jCLV9fQSqWHIVD0F5EXBF2akzmBWhWXfrVwCbSZx7l40GemqC6-Tp_t-gQ0Fnnq2u6Jzai79h_vxlFhweQOXJER8t74RGJgBfGh8Ny2GOaXeJlOQMEe6J3J3iQpp5LyZXq3x2eGKMcAAmG0RqBdg', DATEADD(DAY, 50, @Hoy), '19:30', 'Borrador', DATEADD(DAY,-10,GETDATE())),
 ('EVT-ESP-2026-006', 6, 5, 'Torneo Internacional de Esports Grand Finals',
  'La gran final regional de esports con equipos profesionales.',
  'Final presencial con transmisión en vivo, zona de exhibición de hardware y meet & greet con los equipos finalistas.',
  'Liga Gamer Centroamericana', 'https://lh3.googleusercontent.com/aida-public/AB6AXuAqSpZruLt2ykOPvioe7KeaHI92TC-cqmRpkTyrjcBdJKvlFtit7ATW4o8jrCUfdWMuWYMX9fK20O9wJE2ObJ_mISVKgvGRcfLTz5MD1Vc_YHMFPBAGmkgtqp5hBZExIFlX8FM7YnygBlU2JmmS2WpayyEHmywc7OI7yKBaV-DdY6EQFJMokxQh3p_OmOx9YgItD_WchGuqnvAjtN5K33GY1UVpMV3ChEI-fRv5xSIrvZ3iyoztl9ljvg', DATEADD(DAY, -15, @Hoy), '14:00', 'Finalizado', DATEADD(DAY,-90,GETDATE()));

/* ---------- Tipos de entrada ---------- */
INSERT INTO dbo.TiposEntrada (EventoId, Nombre, Precio, ColorHex, CantidadInicial, CantidadComprometida, CantidadVendida, Activo) VALUES
 (1, 'VIP Front Stage',        180000.00, '#0051D5',  150, 0, 0, 1),
 (1, 'Preferencial',            120000.00, '#316BF3',  600, 0, 0, 1),
 (1, 'General Early Bird',       85000.00, '#7BD0FF', 1500, 0, 0, 1),
 (2, 'Palco Especial',           60000.00, '#0051D5',  300, 0, 0, 1),
 (2, 'Entrada General',          30000.00, '#7BD0FF', 3000, 0, 0, 1),
 (3, 'Platea Preferencial',      55000.00, '#0051D5',  350, 0, 0, 1),
 (3, 'Balcón',                   35000.00, '#7BD0FF',  450, 0, 0, 1),
 (4, 'Acceso Corporativo',       75000.00, '#0051D5',  200, 0, 0, 1),
 (4, 'Estudiante Acreditado',    25000.00, '#7BD0FF',  150, 0, 0, 1),
 (5, 'Platea',                   35000.00, '#316BF3',  700, 0, 0, 1),
 (6, 'General Arena',            35000.00, '#316BF3', 2500, 0, 0, 1);
GO
