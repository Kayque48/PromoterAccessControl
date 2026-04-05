-- ============================================================
-- PromoterCheckIn — Database Schema
-- MySQL
-- ============================================================

CREATE DATABASE IF NOT EXISTS promoter_checkin
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE promoter_checkin;

-- ------------------------------------------------------------
-- EMPRESAS
-- ------------------------------------------------------------
CREATE TABLE empresas (
  id            INT UNSIGNED    NOT NULL AUTO_INCREMENT,
  cnpj          CHAR(14)        NOT NULL,
  razao_social  VARCHAR(150)    NOT NULL,
  nome_fantasia VARCHAR(150)        NULL,
  telefone      VARCHAR(20)     NOT NULL,
  email         VARCHAR(150)    NOT NULL,
  endereco      VARCHAR(255)    NOT NULL,
  ativo         TINYINT(1)      NOT NULL DEFAULT 1,
  criado_em     DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
  atualizado_em DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_empresas_cnpj  (cnpj),
  UNIQUE KEY uq_empresas_email (email)
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- USUÁRIOS
-- ------------------------------------------------------------
CREATE TABLE usuarios (
  id            INT UNSIGNED    NOT NULL AUTO_INCREMENT,
  nome          VARCHAR(100)    NOT NULL,
  login         VARCHAR(150)    NOT NULL,          -- e-mail corporativo
  senha_hash    VARCHAR(255)    NOT NULL,           -- bcrypt / argon2
  telefone      VARCHAR(20)         NULL,
  cargo         VARCHAR(80)         NULL,
  perfil        ENUM('admin','usuario') NOT NULL DEFAULT 'usuario',
  ativo         TINYINT(1)      NOT NULL DEFAULT 1,
  criado_em     DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
  atualizado_em DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_usuarios_login (login)
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- PROMOTORES
-- ------------------------------------------------------------
CREATE TABLE promotores (
  id                     INT UNSIGNED NOT NULL AUTO_INCREMENT,
  cpf                    CHAR(11)     NOT NULL,
  nome                   VARCHAR(100) NOT NULL,
  telefone               VARCHAR(20)      NULL,
  email                  VARCHAR(150)     NULL,
  tipo                   ENUM('promotor','exclusivo') NOT NULL DEFAULT 'promotor',
  -- Para promotor exclusivo, empresa_exclusiva_id fica preenchido
  empresa_exclusiva_id   INT UNSIGNED     NULL,
  ativo                  TINYINT(1)   NOT NULL DEFAULT 1,
  criado_em              DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  atualizado_em          DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_promotores_cpf (cpf),
  CONSTRAINT fk_promotor_empresa_exclusiva
    FOREIGN KEY (empresa_exclusiva_id)
    REFERENCES empresas (id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- VÍNCULO PROMOTOR ↔ EMPRESA  (N:N para promotores não-exclusivos)
-- Dias permitidos ficam aqui pois variam por empresa/promotor
-- ------------------------------------------------------------
CREATE TABLE promotor_empresa (
  id              INT UNSIGNED NOT NULL AUTO_INCREMENT,
  promotor_id     INT UNSIGNED NOT NULL,
  empresa_id      INT UNSIGNED NOT NULL,
  -- Dias da semana permitidos: bitmask (1=Dom,2=Seg,4=Ter,8=Qua,16=Qui,32=Sex,64=Sáb)
  dias_permitidos TINYINT UNSIGNED NOT NULL DEFAULT 62,  -- Seg-Sex por padrão
  ativo           TINYINT(1)   NOT NULL DEFAULT 1,
  criado_em       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_promotor_empresa (promotor_id, empresa_id),
  CONSTRAINT fk_pe_promotor
    FOREIGN KEY (promotor_id) REFERENCES promotores (id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_pe_empresa
    FOREIGN KEY (empresa_id)  REFERENCES empresas (id)
    ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- REGISTROS DE PRESENÇA (movimentações entrada/saída)
-- ------------------------------------------------------------
CREATE TABLE registros (
  id              INT UNSIGNED NOT NULL AUTO_INCREMENT,
  promotor_id     INT UNSIGNED NOT NULL,
  empresa_id      INT UNSIGNED NOT NULL,   -- em nome de qual empresa entrou
  tipo            ENUM('entrada','saida') NOT NULL,
  data_hora       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  -- permanencia_min é calculado na saída (TRIGGER ou app)
  permanencia_min SMALLINT UNSIGNED NULL,
  registrado_por  INT UNSIGNED NOT NULL,   -- FK usuarios.id
  observacao      VARCHAR(255)     NULL,
  PRIMARY KEY (id),
  INDEX idx_reg_promotor   (promotor_id),
  INDEX idx_reg_empresa    (empresa_id),
  INDEX idx_reg_data_hora  (data_hora),
  CONSTRAINT fk_reg_promotor
    FOREIGN KEY (promotor_id)    REFERENCES promotores (id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_reg_empresa
    FOREIGN KEY (empresa_id)     REFERENCES empresas (id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_reg_usuario
    FOREIGN KEY (registrado_por) REFERENCES usuarios (id)
    ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- DOCUMENTOS DO PROMOTOR  (RF02 — comprovante de vínculo)
-- ------------------------------------------------------------
CREATE TABLE promotor_documentos (
  id           INT UNSIGNED NOT NULL AUTO_INCREMENT,
  promotor_id  INT UNSIGNED NOT NULL,
  empresa_id   INT UNSIGNED NOT NULL,
  tipo_doc     VARCHAR(80)  NOT NULL,   -- ex: "Contrato", "Carta de Apresentação"
  caminho      VARCHAR(255) NOT NULL,   -- path/URL do arquivo armazenado
  enviado_em   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  CONSTRAINT fk_doc_promotor
    FOREIGN KEY (promotor_id) REFERENCES promotores (id)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_doc_empresa
    FOREIGN KEY (empresa_id)  REFERENCES empresas (id)
    ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- TRIGGERS
-- ============================================================

DELIMITER $$

-- Impede entrada duplicada em aberto (RF03)
CREATE TRIGGER trg_impede_entrada_duplicada
BEFORE INSERT ON registros
FOR EACH ROW
BEGIN
  DECLARE entrada_aberta INT;

  IF NEW.tipo = 'entrada' THEN
    SELECT COUNT(*) INTO entrada_aberta
    FROM registros
    WHERE promotor_id = NEW.promotor_id
      AND empresa_id  = NEW.empresa_id
      AND tipo        = 'entrada'
      AND DATE(data_hora) = DATE(NEW.data_hora)
      AND id NOT IN (
        SELECT r2.id FROM registros r2
        WHERE r2.promotor_id = NEW.promotor_id
          AND r2.empresa_id  = NEW.empresa_id
          AND r2.tipo        = 'saida'
          AND DATE(r2.data_hora) = DATE(NEW.data_hora)
      );

    IF entrada_aberta > 0 THEN
      SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Já existe uma entrada em aberto para este promotor hoje.';
    END IF;
  END IF;
END$$

-- Calcula permanência automaticamente ao registrar saída (RF03)
CREATE TRIGGER trg_calcula_permanencia
BEFORE INSERT ON registros
FOR EACH ROW
BEGIN
  DECLARE dt_entrada DATETIME;

  IF NEW.tipo = 'saida' THEN
    SELECT data_hora INTO dt_entrada
    FROM registros
    WHERE promotor_id = NEW.promotor_id
      AND empresa_id  = NEW.empresa_id
      AND tipo        = 'entrada'
      AND DATE(data_hora) = DATE(NEW.data_hora)
    ORDER BY data_hora DESC
    LIMIT 1;

    IF dt_entrada IS NULL THEN
      SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Não há entrada válida para registrar a saída.';
    END IF;

    SET NEW.permanencia_min = TIMESTAMPDIFF(MINUTE, dt_entrada, NEW.data_hora);
  END IF;
END$$

-- Limita a 10 promotores exclusivos por empresa (RF04)
CREATE TRIGGER trg_limite_promotores_exclusivos
BEFORE INSERT ON promotores
FOR EACH ROW
BEGIN
  DECLARE total_exclusivos INT;

  IF NEW.tipo = 'exclusivo' AND NEW.empresa_exclusiva_id IS NOT NULL THEN
    SELECT COUNT(*) INTO total_exclusivos
    FROM promotores
    WHERE tipo = 'exclusivo'
      AND empresa_exclusiva_id = NEW.empresa_exclusiva_id
      AND ativo = 1;

    IF total_exclusivos >= 10 THEN
      SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Limite de 10 promotores exclusivos por empresa atingido.';
    END IF;
  END IF;
END$$

-- Garante que promotor exclusivo não seja vinculado a outra empresa (RF02)
CREATE TRIGGER trg_exclusivo_unica_empresa
BEFORE INSERT ON promotor_empresa
FOR EACH ROW
BEGIN
  DECLARE tipo_promotor ENUM('promotor','exclusivo');

  SELECT tipo INTO tipo_promotor
  FROM promotores
  WHERE id = NEW.promotor_id;

  IF tipo_promotor = 'exclusivo' THEN
    SIGNAL SQLSTATE '45000'
      SET MESSAGE_TEXT = 'Promotor exclusivo não pode ser vinculado a mais de uma empresa.';
  END IF;
END$$

DELIMITER ;

-- ============================================================
-- VIEWS úteis para o Dashboard (RF05)
-- ============================================================

-- Promotores presentes no momento
CREATE OR REPLACE VIEW vw_promotores_presentes AS
SELECT
  p.id,
  p.nome,
  p.cpf,
  e.razao_social AS empresa,
  r.data_hora    AS entrada_em
FROM registros r
JOIN promotores p ON p.id = r.promotor_id
JOIN empresas   e ON e.id = r.empresa_id
WHERE r.tipo = 'entrada'
  AND DATE(r.data_hora) = CURDATE()
  AND r.id NOT IN (
    SELECT r2.id
    FROM registros r2
    WHERE r2.promotor_id = r.promotor_id
      AND r2.empresa_id  = r.empresa_id
      AND r2.tipo        = 'saida'
      AND DATE(r2.data_hora) = CURDATE()
  );

-- Tempo médio de permanência por empresa (últimos 30 dias)
CREATE OR REPLACE VIEW vw_media_permanencia AS
SELECT
  e.id            AS empresa_id,
  e.razao_social,
  COUNT(*)        AS total_registros,
  ROUND(AVG(r.permanencia_min), 1) AS media_minutos
FROM registros r
JOIN empresas e ON e.id = r.empresa_id
WHERE r.tipo = 'saida'
  AND r.permanencia_min IS NOT NULL
  AND r.data_hora >= DATE_SUB(NOW(), INTERVAL 30 DAY)
GROUP BY e.id, e.razao_social;