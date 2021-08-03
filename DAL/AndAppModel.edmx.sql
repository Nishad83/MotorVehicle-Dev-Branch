
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 03/10/2021 18:32:39
-- Generated from EDMX file: D:\Projects\And\AndApp\DAL\AndAppModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [ANDAPP];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[ADDONMASTER]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ADDONMASTER];
GO
IF OBJECT_ID(N'[dbo].[INSURANCECOMPANY]', 'U') IS NOT NULL
    DROP TABLE [dbo].[INSURANCECOMPANY];
GO
IF OBJECT_ID(N'[dbo].[INSURANCEVARIANTMASTER]', 'U') IS NOT NULL
    DROP TABLE [dbo].[INSURANCEVARIANTMASTER];
GO
IF OBJECT_ID(N'[dbo].[MAKE_ANDAPP]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MAKE_ANDAPP];
GO
IF OBJECT_ID(N'[dbo].[MODEL_ANDAPP]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MODEL_ANDAPP];
GO
IF OBJECT_ID(N'[dbo].[RTOMASTER]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RTOMASTER];
GO
IF OBJECT_ID(N'[dbo].[STATEMASTER]', 'U') IS NOT NULL
    DROP TABLE [dbo].[STATEMASTER];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'MAKE_ANDAPP'
CREATE TABLE [dbo].[MAKE_ANDAPP] (
    [makeid] bigint  NOT NULL,
    [makename] varchar(50)  NULL,
    [istopmake] bit  NULL,
    [showingorder] bigint  NULL,
    [status] bit  NULL,
    [createdon] datetime  NULL
);
GO

-- Creating table 'MODEL_ANDAPP'
CREATE TABLE [dbo].[MODEL_ANDAPP] (
    [modelid] bigint  NOT NULL,
    [modelname] varchar(100)  NULL,
    [aliasmodelname] varchar(100)  NULL,
    [istopmodel] bit  NULL,
    [modelname_v2] varchar(100)  NULL,
    [makeid] bigint  NULL,
    [status] bit  NULL,
    [createdon] datetime  NULL
);
GO

-- Creating table 'RTOMASTERs'
CREATE TABLE [dbo].[RTOMASTERs] (
    [rtoid] bigint IDENTITY(1,1) NOT NULL,
    [stateid] bigint  NULL,
    [rtocode] varchar(10)  NULL,
    [rtodesc] varchar(50)  NULL,
    [status] bit  NULL
);
GO

-- Creating table 'STATEMASTERs'
CREATE TABLE [dbo].[STATEMASTERs] (
    [stateid] bigint IDENTITY(1,1) NOT NULL,
    [statecode] varchar(50)  NULL,
    [statename] varchar(250)  NULL,
    [zoneid] bigint  NULL,
    [effectivefrom] datetime  NULL,
    [effectiveto] datetime  NULL,
    [status] bit  NULL,
    [createdby] varchar(50)  NULL,
    [createdon] datetime  NULL,
    [updatedby] varchar(50)  NULL,
    [updatedon] datetime  NULL
);
GO

-- Creating table 'INSURANCECOMPANies'
CREATE TABLE [dbo].[INSURANCECOMPANies] (
    [id] bigint IDENTITY(1,1) NOT NULL,
    [companyname] varchar(100)  NULL,
    [createdon] datetime  NULL,
    [status] bit  NULL,
    [isactive] bit  NULL
);
GO

-- Creating table 'ADDONMASTERs'
CREATE TABLE [dbo].[ADDONMASTERs] (
    [addonid] bigint IDENTITY(1,1) NOT NULL,
    [addonname] varchar(50)  NULL,
    [shortname] varchar(20)  NULL,
    [type] varchar(20)  NULL,
    [status] bit  NULL
);
GO

-- Creating table 'INSURANCEVARIANTMASTERs'
CREATE TABLE [dbo].[INSURANCEVARIANTMASTERs] (
    [mastervariant_id] bigint IDENTITY(1,1) NOT NULL,
    [companyid] bigint  NULL,
    [variantname] varchar(50)  NULL,
    [mastermd_id] bigint  NULL,
    [variantid] bigint  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [makeid] in table 'MAKE_ANDAPP'
ALTER TABLE [dbo].[MAKE_ANDAPP]
ADD CONSTRAINT [PK_MAKE_ANDAPP]
    PRIMARY KEY CLUSTERED ([makeid] ASC);
GO

-- Creating primary key on [modelid] in table 'MODEL_ANDAPP'
ALTER TABLE [dbo].[MODEL_ANDAPP]
ADD CONSTRAINT [PK_MODEL_ANDAPP]
    PRIMARY KEY CLUSTERED ([modelid] ASC);
GO

-- Creating primary key on [rtoid] in table 'RTOMASTERs'
ALTER TABLE [dbo].[RTOMASTERs]
ADD CONSTRAINT [PK_RTOMASTERs]
    PRIMARY KEY CLUSTERED ([rtoid] ASC);
GO

-- Creating primary key on [stateid] in table 'STATEMASTERs'
ALTER TABLE [dbo].[STATEMASTERs]
ADD CONSTRAINT [PK_STATEMASTERs]
    PRIMARY KEY CLUSTERED ([stateid] ASC);
GO

-- Creating primary key on [id] in table 'INSURANCECOMPANies'
ALTER TABLE [dbo].[INSURANCECOMPANies]
ADD CONSTRAINT [PK_INSURANCECOMPANies]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [addonid] in table 'ADDONMASTERs'
ALTER TABLE [dbo].[ADDONMASTERs]
ADD CONSTRAINT [PK_ADDONMASTERs]
    PRIMARY KEY CLUSTERED ([addonid] ASC);
GO

-- Creating primary key on [mastervariant_id] in table 'INSURANCEVARIANTMASTERs'
ALTER TABLE [dbo].[INSURANCEVARIANTMASTERs]
ADD CONSTRAINT [PK_INSURANCEVARIANTMASTERs]
    PRIMARY KEY CLUSTERED ([mastervariant_id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------