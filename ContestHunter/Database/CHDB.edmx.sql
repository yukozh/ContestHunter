
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 02/23/2013 00:03:56
-- Generated from EDMX file: C:\Users\Administrator\Desktop\ContestHunter\ContestHunter\Database\CHDB.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [test];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_CONTEST_ATTEND_CONTEST]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CONTEST_ATTEND] DROP CONSTRAINT [FK_CONTEST_ATTEND_CONTEST];
GO
IF OBJECT_ID(N'[dbo].[FK_CONTEST_ATTEND_USER]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CONTEST_ATTEND] DROP CONSTRAINT [FK_CONTEST_ATTEND_USER];
GO
IF OBJECT_ID(N'[dbo].[FK_CONTEST_OWNER_CONTEST]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CONTEST_OWNER] DROP CONSTRAINT [FK_CONTEST_OWNER_CONTEST];
GO
IF OBJECT_ID(N'[dbo].[FK_CONTEST_OWNER_USER]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CONTEST_OWNER] DROP CONSTRAINT [FK_CONTEST_OWNER_USER];
GO
IF OBJECT_ID(N'[dbo].[FK_HACK_RECORD]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[HUNT] DROP CONSTRAINT [FK_HACK_RECORD];
GO
IF OBJECT_ID(N'[dbo].[FK_HACK_USER]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[HUNT] DROP CONSTRAINT [FK_HACK_USER];
GO
IF OBJECT_ID(N'[dbo].[FK_PROBLEM_CONTEST1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PROBLEM] DROP CONSTRAINT [FK_PROBLEM_CONTEST1];
GO
IF OBJECT_ID(N'[dbo].[FK_PROBLEM_USER]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PROBLEM] DROP CONSTRAINT [FK_PROBLEM_USER];
GO
IF OBJECT_ID(N'[dbo].[FK_Rating_CONTEST]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RATING] DROP CONSTRAINT [FK_Rating_CONTEST];
GO
IF OBJECT_ID(N'[dbo].[FK_Rating_USER]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RATING] DROP CONSTRAINT [FK_Rating_USER];
GO
IF OBJECT_ID(N'[dbo].[FK_RECORD_PROBLEM]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RECORD] DROP CONSTRAINT [FK_RECORD_PROBLEM];
GO
IF OBJECT_ID(N'[dbo].[FK_RECORD_USER]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RECORD] DROP CONSTRAINT [FK_RECORD_USER];
GO
IF OBJECT_ID(N'[dbo].[FK_TESTDATA_PROBLEM]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TESTDATA] DROP CONSTRAINT [FK_TESTDATA_PROBLEM];
GO
IF OBJECT_ID(N'[dbo].[FK_USER_GROUP_GROUP]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[USER_GROUP] DROP CONSTRAINT [FK_USER_GROUP_GROUP];
GO
IF OBJECT_ID(N'[dbo].[FK_USER_GROUP_USER]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[USER_GROUP] DROP CONSTRAINT [FK_USER_GROUP_USER];
GO
IF OBJECT_ID(N'[dbo].[FK_USER_LOCK_PROBLEM]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[LOCK] DROP CONSTRAINT [FK_USER_LOCK_PROBLEM];
GO
IF OBJECT_ID(N'[dbo].[FK_USER_LOCK_USER]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[LOCK] DROP CONSTRAINT [FK_USER_LOCK_USER];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[CONTEST]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CONTEST];
GO
IF OBJECT_ID(N'[dbo].[CONTEST_ATTEND]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CONTEST_ATTEND];
GO
IF OBJECT_ID(N'[dbo].[CONTEST_OWNER]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CONTEST_OWNER];
GO
IF OBJECT_ID(N'[dbo].[GROUP]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GROUP];
GO
IF OBJECT_ID(N'[dbo].[HUNT]', 'U') IS NOT NULL
    DROP TABLE [dbo].[HUNT];
GO
IF OBJECT_ID(N'[dbo].[LOCK]', 'U') IS NOT NULL
    DROP TABLE [dbo].[LOCK];
GO
IF OBJECT_ID(N'[dbo].[PROBLEM]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PROBLEM];
GO
IF OBJECT_ID(N'[dbo].[RATING]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RATING];
GO
IF OBJECT_ID(N'[dbo].[RECORD]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RECORD];
GO
IF OBJECT_ID(N'[dbo].[TESTDATA]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TESTDATA];
GO
IF OBJECT_ID(N'[dbo].[USER]', 'U') IS NOT NULL
    DROP TABLE [dbo].[USER];
GO
IF OBJECT_ID(N'[dbo].[USER_GROUP]', 'U') IS NOT NULL
    DROP TABLE [dbo].[USER_GROUP];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'CONTESTs'
CREATE TABLE [dbo].[CONTESTs] (
    [ID] guid  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [StartTime] datetime  NOT NULL,
    [EndTime] datetime  NOT NULL,
    [Description] longtext  NOT NULL,
    [Type] int  NOT NULL,
    [IsOfficial] bool  NOT NULL,
    [Status] int  NOT NULL
);
GO

-- Creating table 'CONTEST_ATTEND'
CREATE TABLE [dbo].[CONTEST_ATTEND] (
    [Contest] guid  NOT NULL,
    [User] guid  NOT NULL,
    [Type] int  NOT NULL,
    [Time] datetime  NULL
);
GO

-- Creating table 'GROUPs'
CREATE TABLE [dbo].[GROUPs] (
    [ID] guid  NOT NULL,
    [Name] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'HUNTs'
CREATE TABLE [dbo].[HUNTs] (
    [User] guid  NOT NULL,
    [Record] guid  NOT NULL,
    [HuntData] longblob  NOT NULL,
    [Time] datetime  NOT NULL,
    [Status] int  NOT NULL,
    [Detail] longtext  NULL
);
GO

-- Creating table 'PROBLEMs'
CREATE TABLE [dbo].[PROBLEMs] (
    [ID] guid  NOT NULL,
    [Contest] guid  NOT NULL,
    [User] guid  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [Content] longtext  NOT NULL,
    [Comparer] longtext  NOT NULL,
    [ComparerLanguage] int  NOT NULL,
    [DataChecker] longtext  NOT NULL,
    [DataCheckerLanguage] int  NOT NULL,
    [OriginRating] int  NULL
);
GO

-- Creating table 'RATINGs'
CREATE TABLE [dbo].[RATINGs] (
    [User] guid  NOT NULL,
    [Contest] guid  NOT NULL,
    [Rating1] int  NOT NULL
);
GO

-- Creating table 'RECORDs'
CREATE TABLE [dbo].[RECORDs] (
    [ID] guid  NOT NULL,
    [User] guid  NOT NULL,
    [Problem] guid  NOT NULL,
    [Code] longtext  NOT NULL,
    [Language] int  NOT NULL,
    [SubmitTime] datetime  NOT NULL,
    [VirtualSubmitTime] datetime  NOT NULL,
    [ExecutedTime] int  NULL,
    [MemoryUsed] bigint  NULL,
    [CodeLength] int  NOT NULL,
    [Status] int  NOT NULL,
    [Score] int  NULL,
    [Detail] longtext  NULL
);
GO

-- Creating table 'TESTDATAs'
CREATE TABLE [dbo].[TESTDATAs] (
    [ID] guid  NOT NULL,
    [Problem] guid  NOT NULL,
    [Input] longblob  NOT NULL,
    [Data] longblob  NOT NULL,
    [TimeLimit] int  NOT NULL,
    [MemoryLimit] bigint  NOT NULL,
    [Available] bool  NOT NULL
);
GO

-- Creating table 'USERs'
CREATE TABLE [dbo].[USERs] (
    [ID] guid  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [Password] tinyblob  NOT NULL,
    [Email] nvarchar(100)  NOT NULL,
    [RealName] nvarchar(50)  NULL,
    [Country] nvarchar(50)  NULL,
    [Province] nvarchar(50)  NULL,
    [City] nvarchar(50)  NULL,
    [School] nvarchar(50)  NULL,
    [LastLoginTime] datetime  NULL,
    [LastLoginIP] nvarchar(50)  NULL,
    [Motto] nvarchar(100)  NULL
);
GO

-- Creating table 'CONTEST_OWNER'
CREATE TABLE [dbo].[CONTEST_OWNER] (
    [CONTESTs_ID] guid  NOT NULL,
    [OWNERs_ID] guid  NOT NULL
);
GO

-- Creating table 'LOCK'
CREATE TABLE [dbo].[LOCK] (
    [LOCKs_ID] guid  NOT NULL,
    [USERs_ID] guid  NOT NULL
);
GO

-- Creating table 'USER_GROUP'
CREATE TABLE [dbo].[USER_GROUP] (
    [GROUPs_ID] guid  NOT NULL,
    [USERs_ID] guid  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [ID] in table 'CONTESTs'
ALTER TABLE [dbo].[CONTESTs]
ADD CONSTRAINT [PK_CONTESTs]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [Contest], [User] in table 'CONTEST_ATTEND'
ALTER TABLE [dbo].[CONTEST_ATTEND]
ADD CONSTRAINT [PK_CONTEST_ATTEND]
    PRIMARY KEY CLUSTERED ([Contest], [User] ASC);
GO

-- Creating primary key on [ID] in table 'GROUPs'
ALTER TABLE [dbo].[GROUPs]
ADD CONSTRAINT [PK_GROUPs]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [User], [Record] in table 'HUNTs'
ALTER TABLE [dbo].[HUNTs]
ADD CONSTRAINT [PK_HUNTs]
    PRIMARY KEY CLUSTERED ([User], [Record] ASC);
GO

-- Creating primary key on [ID] in table 'PROBLEMs'
ALTER TABLE [dbo].[PROBLEMs]
ADD CONSTRAINT [PK_PROBLEMs]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [User], [Contest] in table 'RATINGs'
ALTER TABLE [dbo].[RATINGs]
ADD CONSTRAINT [PK_RATINGs]
    PRIMARY KEY CLUSTERED ([User], [Contest] ASC);
GO

-- Creating primary key on [ID] in table 'RECORDs'
ALTER TABLE [dbo].[RECORDs]
ADD CONSTRAINT [PK_RECORDs]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'TESTDATAs'
ALTER TABLE [dbo].[TESTDATAs]
ADD CONSTRAINT [PK_TESTDATAs]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'USERs'
ALTER TABLE [dbo].[USERs]
ADD CONSTRAINT [PK_USERs]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [CONTESTs_ID], [OWNERs_ID] in table 'CONTEST_OWNER'
ALTER TABLE [dbo].[CONTEST_OWNER]
ADD CONSTRAINT [PK_CONTEST_OWNER]
    PRIMARY KEY NONCLUSTERED ([CONTESTs_ID], [OWNERs_ID] ASC);
GO

-- Creating primary key on [LOCKs_ID], [USERs_ID] in table 'LOCK'
ALTER TABLE [dbo].[LOCK]
ADD CONSTRAINT [PK_LOCK]
    PRIMARY KEY NONCLUSTERED ([LOCKs_ID], [USERs_ID] ASC);
GO

-- Creating primary key on [GROUPs_ID], [USERs_ID] in table 'USER_GROUP'
ALTER TABLE [dbo].[USER_GROUP]
ADD CONSTRAINT [PK_USER_GROUP]
    PRIMARY KEY NONCLUSTERED ([GROUPs_ID], [USERs_ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Contest] in table 'CONTEST_ATTEND'
ALTER TABLE [dbo].[CONTEST_ATTEND]
ADD CONSTRAINT [FK_CONTEST_ATTEND_CONTEST]
    FOREIGN KEY ([Contest])
    REFERENCES [dbo].[CONTESTs]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Contest] in table 'PROBLEMs'
ALTER TABLE [dbo].[PROBLEMs]
ADD CONSTRAINT [FK_PROBLEM_CONTEST1]
    FOREIGN KEY ([Contest])
    REFERENCES [dbo].[CONTESTs]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PROBLEM_CONTEST1'
CREATE INDEX [IX_FK_PROBLEM_CONTEST1]
ON [dbo].[PROBLEMs]
    ([Contest]);
GO

-- Creating foreign key on [Contest] in table 'RATINGs'
ALTER TABLE [dbo].[RATINGs]
ADD CONSTRAINT [FK_Rating_CONTEST]
    FOREIGN KEY ([Contest])
    REFERENCES [dbo].[CONTESTs]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Rating_CONTEST'
CREATE INDEX [IX_FK_Rating_CONTEST]
ON [dbo].[RATINGs]
    ([Contest]);
GO

-- Creating foreign key on [User] in table 'CONTEST_ATTEND'
ALTER TABLE [dbo].[CONTEST_ATTEND]
ADD CONSTRAINT [FK_CONTEST_ATTEND_USER]
    FOREIGN KEY ([User])
    REFERENCES [dbo].[USERs]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CONTEST_ATTEND_USER'
CREATE INDEX [IX_FK_CONTEST_ATTEND_USER]
ON [dbo].[CONTEST_ATTEND]
    ([User]);
GO

-- Creating foreign key on [Record] in table 'HUNTs'
ALTER TABLE [dbo].[HUNTs]
ADD CONSTRAINT [FK_HACK_RECORD]
    FOREIGN KEY ([Record])
    REFERENCES [dbo].[RECORDs]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_HACK_RECORD'
CREATE INDEX [IX_FK_HACK_RECORD]
ON [dbo].[HUNTs]
    ([Record]);
GO

-- Creating foreign key on [User] in table 'HUNTs'
ALTER TABLE [dbo].[HUNTs]
ADD CONSTRAINT [FK_HACK_USER]
    FOREIGN KEY ([User])
    REFERENCES [dbo].[USERs]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [User] in table 'PROBLEMs'
ALTER TABLE [dbo].[PROBLEMs]
ADD CONSTRAINT [FK_PROBLEM_USER]
    FOREIGN KEY ([User])
    REFERENCES [dbo].[USERs]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PROBLEM_USER'
CREATE INDEX [IX_FK_PROBLEM_USER]
ON [dbo].[PROBLEMs]
    ([User]);
GO

-- Creating foreign key on [Problem] in table 'RECORDs'
ALTER TABLE [dbo].[RECORDs]
ADD CONSTRAINT [FK_RECORD_PROBLEM]
    FOREIGN KEY ([Problem])
    REFERENCES [dbo].[PROBLEMs]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RECORD_PROBLEM'
CREATE INDEX [IX_FK_RECORD_PROBLEM]
ON [dbo].[RECORDs]
    ([Problem]);
GO

-- Creating foreign key on [Problem] in table 'TESTDATAs'
ALTER TABLE [dbo].[TESTDATAs]
ADD CONSTRAINT [FK_TESTDATA_PROBLEM]
    FOREIGN KEY ([Problem])
    REFERENCES [dbo].[PROBLEMs]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TESTDATA_PROBLEM'
CREATE INDEX [IX_FK_TESTDATA_PROBLEM]
ON [dbo].[TESTDATAs]
    ([Problem]);
GO

-- Creating foreign key on [User] in table 'RATINGs'
ALTER TABLE [dbo].[RATINGs]
ADD CONSTRAINT [FK_Rating_USER]
    FOREIGN KEY ([User])
    REFERENCES [dbo].[USERs]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [User] in table 'RECORDs'
ALTER TABLE [dbo].[RECORDs]
ADD CONSTRAINT [FK_RECORD_USER]
    FOREIGN KEY ([User])
    REFERENCES [dbo].[USERs]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RECORD_USER'
CREATE INDEX [IX_FK_RECORD_USER]
ON [dbo].[RECORDs]
    ([User]);
GO

-- Creating foreign key on [CONTESTs_ID] in table 'CONTEST_OWNER'
ALTER TABLE [dbo].[CONTEST_OWNER]
ADD CONSTRAINT [FK_CONTEST_OWNER_CONTEST]
    FOREIGN KEY ([CONTESTs_ID])
    REFERENCES [dbo].[CONTESTs]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [OWNERs_ID] in table 'CONTEST_OWNER'
ALTER TABLE [dbo].[CONTEST_OWNER]
ADD CONSTRAINT [FK_CONTEST_OWNER_USER]
    FOREIGN KEY ([OWNERs_ID])
    REFERENCES [dbo].[USERs]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CONTEST_OWNER_USER'
CREATE INDEX [IX_FK_CONTEST_OWNER_USER]
ON [dbo].[CONTEST_OWNER]
    ([OWNERs_ID]);
GO

-- Creating foreign key on [LOCKs_ID] in table 'LOCK'
ALTER TABLE [dbo].[LOCK]
ADD CONSTRAINT [FK_LOCK_PROBLEM]
    FOREIGN KEY ([LOCKs_ID])
    REFERENCES [dbo].[PROBLEMs]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [USERs_ID] in table 'LOCK'
ALTER TABLE [dbo].[LOCK]
ADD CONSTRAINT [FK_LOCK_USER]
    FOREIGN KEY ([USERs_ID])
    REFERENCES [dbo].[USERs]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_LOCK_USER'
CREATE INDEX [IX_FK_LOCK_USER]
ON [dbo].[LOCK]
    ([USERs_ID]);
GO

-- Creating foreign key on [GROUPs_ID] in table 'USER_GROUP'
ALTER TABLE [dbo].[USER_GROUP]
ADD CONSTRAINT [FK_USER_GROUP_GROUP]
    FOREIGN KEY ([GROUPs_ID])
    REFERENCES [dbo].[GROUPs]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [USERs_ID] in table 'USER_GROUP'
ALTER TABLE [dbo].[USER_GROUP]
ADD CONSTRAINT [FK_USER_GROUP_USER]
    FOREIGN KEY ([USERs_ID])
    REFERENCES [dbo].[USERs]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_USER_GROUP_USER'
CREATE INDEX [IX_FK_USER_GROUP_USER]
ON [dbo].[USER_GROUP]
    ([USERs_ID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------