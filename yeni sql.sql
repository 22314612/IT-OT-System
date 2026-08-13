-- 1. Departments Tablosu
CREATE TABLE Departments (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(MAX) NULL
);

-- 2. Users Tablosu
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Fullname NVARCHAR(MAX) NULL,
    Email NVARCHAR(MAX) NULL,
    Password NVARCHAR(MAX) NULL,
    Role NVARCHAR(MAX) NULL,
    DepartmentId INT NULL,
    CONSTRAINT FK_Users_Departments_DepartmentId FOREIGN KEY (DepartmentId) 
        REFERENCES Departments (DepartmentId) ON DELETE SET NULL
);

-- 3. Records Tablosu
CREATE TABLE Records (
    RecordId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(MAX) NULL,
    Description NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2 NOT NULL,
    TargetDepartmentId INT NULL,
    CONSTRAINT FK_Records_Departments_TargetDepartmentId FOREIGN KEY (TargetDepartmentId) 
        REFERENCES Departments (DepartmentId) ON DELETE NO ACTION
);