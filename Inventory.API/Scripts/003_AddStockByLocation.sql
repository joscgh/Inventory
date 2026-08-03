-- Existencias por depósito / tienda, y depósito+tienda en notas y ajustes.
-- Se aplica a mano porque el esquema vivo se creó con EnsureCreated() y no sigue
-- el historial de migraciones de EF Core.
--
--   psql -h localhost -p 5432 -U postgres -d InventoryDb -f 003_AddStockByLocation.sql

BEGIN;

-- 1. Existencias por ubicación.
CREATE TABLE IF NOT EXISTS "ItemStocks" (
    "ItemId" integer NOT NULL,
    "LocationId" integer NOT NULL,
    "Quantity" double precision NOT NULL DEFAULT 0,
    CONSTRAINT "PK_ItemStocks" PRIMARY KEY ("ItemId", "LocationId"),
    CONSTRAINT "FK_ItemStocks_Items_ItemId"
        FOREIGN KEY ("ItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ItemStocks_AccountLocations_LocationId"
        FOREIGN KEY ("LocationId") REFERENCES "AccountLocations" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_ItemStocks_LocationId" ON "ItemStocks" ("LocationId");

-- 2. Depósito y tienda de cada nota.
ALTER TABLE "Notes" ADD COLUMN IF NOT EXISTS "WarehouseId" integer;
ALTER TABLE "Notes" ADD COLUMN IF NOT EXISTS "StoreId" integer;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Notes_AccountLocations_WarehouseId') THEN
        ALTER TABLE "Notes" ADD CONSTRAINT "FK_Notes_AccountLocations_WarehouseId"
            FOREIGN KEY ("WarehouseId") REFERENCES "AccountLocations" ("Id") ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Notes_AccountLocations_StoreId') THEN
        ALTER TABLE "Notes" ADD CONSTRAINT "FK_Notes_AccountLocations_StoreId"
            FOREIGN KEY ("StoreId") REFERENCES "AccountLocations" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS "IX_Notes_WarehouseId" ON "Notes" ("WarehouseId");
CREATE INDEX IF NOT EXISTS "IX_Notes_StoreId" ON "Notes" ("StoreId");

-- 3. Ubicación de cada movimiento del historial.
ALTER TABLE "Adjustments" ADD COLUMN IF NOT EXISTS "LocationId" integer;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Adjustments_AccountLocations_LocationId') THEN
        ALTER TABLE "Adjustments" ADD CONSTRAINT "FK_Adjustments_AccountLocations_LocationId"
            FOREIGN KEY ("LocationId") REFERENCES "AccountLocations" ("Id") ON DELETE SET NULL;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS "IX_Adjustments_LocationId" ON "Adjustments" ("LocationId");

-- 4. Migrar el stock que hoy vive en Items."Stock" a un depósito concreto.
--    Sólo es automático si hay exactamente un depósito; con varios hay que decidir
--    a mano y el script se detiene sin tocar nada.
DO $$
DECLARE
    warehouse_count integer;
    target_id integer;
    migrated integer;
BEGIN
    SELECT COUNT(*), MIN("Id") INTO warehouse_count, target_id
    FROM "AccountLocations" WHERE "Type" = 0;

    IF warehouse_count = 0 THEN
        RAISE NOTICE 'No hay depositos registrados: no se migro stock. Registra un deposito y vuelve a correr esta seccion.';
    ELSIF warehouse_count > 1 THEN
        RAISE EXCEPTION 'Hay % depositos registrados. Edita este script y fija target_id a mano antes de migrar.', warehouse_count;
    ELSE
        INSERT INTO "ItemStocks" ("ItemId", "LocationId", "Quantity")
        SELECT i."Id", target_id, i."Stock"
        FROM "Items" i
        WHERE NOT EXISTS (SELECT 1 FROM "ItemStocks" s WHERE s."ItemId" = i."Id")
        ON CONFLICT DO NOTHING;

        GET DIAGNOSTICS migrated = ROW_COUNT;
        RAISE NOTICE 'Stock de % articulos migrado al deposito id=%.', migrated, target_id;
    END IF;
END $$;

COMMIT;
