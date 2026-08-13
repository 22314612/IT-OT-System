DROP TABLE IF EXISTS Records;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS Departments;

CREATE TABLE Departments (
    departmentId   INT PRIMARY KEY,
    DeparmentName  VARCHAR(100) NOT NULL,
    SupervisorId   INT NOT NULL
);

CREATE TABLE Users (
    UserId         INT PRIMARY KEY, 
    Fullname       VARCHAR(100) NOT NULL,
    Email          VARCHAR(100) UNIQUE NOT NULL,
    Password       VARCHAR(50) NOT NULL, -- Şifre kolonu eklendi!
    Role           VARCHAR(50), 
    departmentId   INT NOT NULL,
    phone          VARCHAR(15), 
    FOREIGN KEY (departmentId) REFERENCES Departments (departmentId)
);

CREATE TABLE Records(
    RecordId        INT PRIMARY KEY,
    Title           VARCHAR(150) NOT NULL,
    Content         VARCHAR(150) NOT NULL,
    RecordType      VARCHAR(150) NOT NULL,
    Status          VARCHAR(20) NOT NULL,
    FeedbackText    VARCHAR(150) NULL,
    CreatedByUserId INT NOT NULL,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users (UserId)
);

-- Veri Ekleme İşlemleri
INSERT INTO Departments(DeparmentName, SupervisorId, departmentId) VALUES
    ('Üretim', 1, 41), ('Bakım', 2, 13), ('Insan Kaynakları', 3, 72),
    ('IT', 4, 44), ('Logistik', 5, 24), ('Kalite', 6, 96),
    ('ISG', 7, 87), ('Satın Alma', 8, 36), ('Depo Yönetimi', 9, 54),
    ('ARGE', 10, 12), ('Planlama', 11, 22);

INSERT INTO Users(userid, departmentid, Fullname, Password, Role, phone, Email) VALUES
    (1012, 41, 'Tayfur Bingöl', '1234', 'CNC Operatörü', '511111111', 'tayfur.bingöl@gmail.com'),
    (2085, 13, 'Uğurcan Çakır', '1234', 'Bakım Teknisyeni', '5222222222', 'ugurcan.cakir@gmail.com'),
    (1102, 72, 'Barış Alper Yılmaz', '1234', 'Ik Yardımcısı', '5333333333', 'barisalper.yilmaz@gmail.com'),
    (1674, 44, 'Volkan Babacan', '1234', 'Developer', '54444444444', 'volkan.babacan@gmail.com'),
    (2436, 24, 'Mert Günok', '1234', 'Endüstri Mühendisi', '5555555555', 'mert.gunok@gmail.com'),
    (5945, 96, 'Kerem Aktürkoğlu', '1234', 'Endüstri Mühendisi', '5666666666', 'kerem.akturkoglu@gmail.com');

INSERT INTO Records(recordid, recordtype, createdbyuserid, title, content, status, feedbacktext) VALUES
    (1, 'Suggest', 2085, 'Bakım', 'bakımmüdürü', 'active', 'Yapılmadı'),
    (2, 'Arıza Bildirimi', 2085, 'Kompresör Arızası', 'Atölye kompresöründe basınç düşüklüğü tespit edildi.', 'İnceleniyor', 'Teknik ekip yönlendirildi.'),
    (3, 'Talep', 1102, 'Personel Eğitim Talebi', 'Yeni başlayan çalışanlar için eğitim planlanması isteniyor.', 'Açık', NULL);