-- Önce IDENTITY_INSERT özelliğini açıyoruz ki ID'leri kendimiz yazabilelim
SET IDENTITY_INSERT Departments ON;

-- Görseldeki Departmanlar (ID'leri birebir eşleşecek şekilde)
INSERT INTO Departments (DepartmentId, DepartmentName) VALUES 
(4, 'Üretim (Production)'),
(5, 'Bakım / Arıza (Maintenance)'),
(6, 'Kalite Kontrol (Quality Assurance)'),
(7, 'Bilgi İşlem (IT)'),
(8, 'İnsan Kaynakları (HR)'),
(9, 'Lojistik ve Depo (Logistics)');

-- İşimiz bitince kapatıyoruz
SET IDENTITY_INSERT Departments OFF;


-- Şimdi Görseldeki Personelleri Uygun Departman ID'leriyle Ekleyelim
INSERT INTO Users (Fullname, Email, Password, Role, DepartmentId) VALUES 
-- Üretim (ID: 4)
('Kadir Tanhan', 'Kadir@gmail.com', '1234', 'Supervisor', 4),

-- Bakım / Arıza (ID: 5)
('Gizem Abla', 'gizem@gmail.com', '1234', 'Supervisor', 5),

-- Kalite Kontrol (ID: 6)
('İsmail Göker', 'ismail@gmail.com', '1234', 'Supervisor', 6),

-- Bilgi İşlem (ID: 7)
('Mert Aktan Çeliker', 'aktan@gmail.com', '1234', 'Supervisor', 7),
('Admin', 'admin@gmail.com', '1234', 'Admin', 7),
('agah', 'agah@gmail.com', '1234', 'User', 7),

-- İnsan Kaynakları (ID: 8)
('Nilsu Aygül', 'nilsu@gmail.com', '1234', 'Supervisor', 8),
('Yusuf Çiçek', 'ysuuf@gmail.com', '1234', 'Supervisor', 8),

-- Lojistik ve Depo (ID: 9)
('Seher Usta', 'seher@gmail.com', '1234', 'Supervisor', 9),
('Eralp Yılmaz', 'eralp@gmail.com', '1234', 'User', 9);