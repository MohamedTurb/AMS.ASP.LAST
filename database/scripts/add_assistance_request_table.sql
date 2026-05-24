-- Create AssistanceRequests table
CREATE TABLE IF NOT EXISTS "AssistanceRequests" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AssistanceRequests" PRIMARY KEY AUTOINCREMENT,
    "FullName" TEXT NOT NULL,
    "NationalId" TEXT NOT NULL,
    "PhoneNumber" TEXT NOT NULL,
    "Address" TEXT NOT NULL,
    "TypeOfAssistance" TEXT NOT NULL,
    "Amount" decimal(18,2) NULL,
    "Reason" TEXT NULL,
    "SupportingDocuments" TEXT NULL,
    "Status" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL,
    "CreatedByUserId" TEXT NULL,
    "ReviewedByUserId" TEXT NULL,
    "ReviewedAt" TEXT NULL,
    "ReviewNotes" TEXT NULL,
    CONSTRAINT "FK_AssistanceRequests_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_AssistanceRequests_AspNetUsers_ReviewedByUserId" FOREIGN KEY ("ReviewedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE SET NULL
);

-- Create unique index for NationalId
CREATE UNIQUE INDEX IF NOT EXISTS "IX_AssistanceRequests_NationalId" ON "AssistanceRequests" ("NationalId");

-- Create indexes for foreign keys
CREATE INDEX IF NOT EXISTS "IX_AssistanceRequests_CreatedByUserId" ON "AssistanceRequests" ("CreatedByUserId");
CREATE INDEX IF NOT EXISTS "IX_AssistanceRequests_ReviewedByUserId" ON "AssistanceRequests" ("ReviewedByUserId");
