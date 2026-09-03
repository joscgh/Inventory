-- Logo del cliente, en tabla aparte (1:1 con la cuenta) para que listar cuentas
-- no tenga que traer los bytes de todas las imágenes.
-- Se aplica a mano porque el esquema vivo se creó con EnsureCreated() y no sigue
-- el historial de migraciones de EF Core.
--
--   psql -h localhost -p 5432 -U postgres -d InventoryDb -f 002_AddAccountLogos.sql

CREATE TABLE IF NOT EXISTS "AccountLogos" (
    "CustomerAccountId" integer NOT NULL,
    "ContentType" text NOT NULL,
    "FileName" text NOT NULL,
    "Data" bytea NOT NULL,
    CONSTRAINT "PK_AccountLogos" PRIMARY KEY ("CustomerAccountId"),
    CONSTRAINT "FK_AccountLogos_CustomerAccounts_CustomerAccountId"
        FOREIGN KEY ("CustomerAccountId") REFERENCES "CustomerAccounts" ("Id") ON DELETE CASCADE
);
