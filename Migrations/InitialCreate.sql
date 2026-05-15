IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Clients] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(120) NOT NULL,
    [ContactDetails] nvarchar(150) NOT NULL,
    [Region] nvarchar(80) NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Contracts] (
    [Id] int NOT NULL IDENTITY,
    [ClientId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [ServiceLevel] nvarchar(200) NOT NULL,
    [SignedAgreementFileName] nvarchar(255) NULL,
    [SignedAgreementStoredPath] nvarchar(500) NULL,
    [SignedAgreementContentType] nvarchar(100) NULL,
    [SignedAgreementUploadedAt] datetime2 NULL,
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Contracts_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ServiceRequests] (
    [Id] int NOT NULL IDENTITY,
    [ContractId] int NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [CostUsd] decimal(18,2) NOT NULL,
    [CostZar] decimal(18,2) NOT NULL,
    [ExchangeRateUsed] decimal(18,6) NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ServiceRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ServiceRequests_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_Contracts_ClientId] ON [Contracts] ([ClientId]);
GO

CREATE INDEX [IX_ServiceRequests_ContractId] ON [ServiceRequests] ([ContractId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260515084606_InitialCreate', N'8.0.0');
GO

COMMIT;
GO

