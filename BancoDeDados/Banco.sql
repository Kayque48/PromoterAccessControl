-- Criação do banco de dados
CREATE DATABASE IF NOT EXISTS controlepromotores;
USE controlepromotores;

-- Tabela de empresas
CREATE TABLE Empresas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(255) NOT NULL,
    Telefone VARCHAR(20),
    Endereco VARCHAR(255)
);

-- Tabela de promotores
CREATE TABLE Promotores (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(255) NOT NULL,
    Telefone VARCHAR(20),
    Email VARCHAR(255),
    Ativo BOOLEAN DEFAULT TRUE,
    EmpresaId INT,
    FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id) ON DELETE SET NULL
);

-- Tabela de registros de acesso
CREATE TABLE RegistrosAcesso (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Entrada DATETIME NOT NULL,
    Saida DATETIME,
    TempoPermanencia INT, -- em minutos
    PromotorId INT NOT NULL,
    FOREIGN KEY (PromotorId) REFERENCES Promotores(Id) ON DELETE CASCADE
);

-- Tabela de usuários do sistema
CREATE TABLE Usuarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(255) NOT NULL,
    Login VARCHAR(100) NOT NULL UNIQUE,
    SenhaHash VARCHAR(255) NOT NULL,
    Perfil VARCHAR(50) DEFAULT 'operador'
);

-- Inserir uma empresa padrão
INSERT INTO Empresas (Nome) VALUES ('Empresa Padrão');


INSERT INTO Usuarios (Nome, Login, SenhaHash, Perfil) 
VALUES ('Administrador', 'admin', '$2a$12$NW3iAtMfiD1kMCeok3BsJuwMuyPLmpi9tkdS0SkHr.APjHT9JZpti', 'admin');

-- Opcional: inserir algumas empresas e promotores de exemplo
INSERT INTO Empresas (Nome, Telefone) VALUES ('Distribuidora A', '(11) 1234-5678');
INSERT INTO Empresas (Nome, Telefone) VALUES ('Representações B', '(21) 9876-5432');

INSERT INTO Promotores (Nome, Telefone, Email, EmpresaId) VALUES 
('João Silva', '(11) 91234-5678', 'joao@exemplo.com', 1),
('Maria Santos', '(21) 97654-3210', 'maria@exemplo.com', 2);

-- Inserir alguns registros de exemplo (últimos 30 dias)
INSERT INTO RegistrosAcesso (Entrada, Saida, TempoPermanencia, PromotorId) VALUES
('2025-02-01 08:30:00', '2025-02-01 12:45:00', 255, 1),
('2025-02-01 09:15:00', '2025-02-01 11:30:00', 135, 2),
('2025-02-02 08:00:00', '2025-02-02 12:00:00', 240, 1),
('2025-02-02 10:00:00', '2025-02-02 15:00:00', 300, 2),
('2025-02-03 08:45:00', '2025-02-03 12:15:00', 210, 1);