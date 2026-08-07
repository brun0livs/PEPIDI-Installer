-- ==============================================================
-- PEPIDI -- Script completo de instalacao
-- Schema + Stored Procedures + Triggers + Dados Demo
-- ==============================================================

USE master;
GO
IF DB_ID(N'PEPIDI') IS NULL
    CREATE DATABASE [PEPIDI] COLLATE Latin1_General_CI_AI;
GO
USE [PEPIDI];
GO

-- ==============================================================
-- PARTE 1: TABELAS
-- ==============================================================

CREATE TABLE [Cor] (
    [ID] int NOT NULL,
    [Nome] nchar(16) NOT NULL,
    CONSTRAINT [PK_Cor] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [Estado] (
    [ID] int NOT NULL,
    [Descricao] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Estado] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [Estabelecimentos] (
    [ID] int NOT NULL,
    [Nome] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Estab_1] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [Acessos] (
    [ID] int NOT NULL,
    [Descricao] nvarchar(100) NOT NULL,
    CONSTRAINT [PK__Acessos__3214EC278772ACA7] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [Funcoes] (
    [ID] int NOT NULL,
    [Nome] nvarchar(50) NOT NULL,
    [NivelAcesso] int NOT NULL,
    [PodeVerStock] bit NOT NULL DEFAULT ((0)),
    [PodeInserirStock] bit NOT NULL,
    [PodeCriarStock] bit NOT NULL,
    [PodeVerHistorico] bit NOT NULL DEFAULT ((0)),
    [PodeEditarFunc] bit NOT NULL DEFAULT ((0)),
    [PodeSubmeter] bit NOT NULL DEFAULT ((1)),
    [PodeAprovar] bit NOT NULL DEFAULT ((0)),
    [PodeEntregar] bit NOT NULL DEFAULT ((0)),
    [PodeCriarFuncoes] bit NOT NULL DEFAULT ((0)),
    [PodeAlterarDefinicoes] bit NOT NULL DEFAULT ((0)),
    [PodeVerUsados] bit NOT NULL,
    [CorHex] nchar(10) NOT NULL,
    [CriadoEm] datetime2 NOT NULL DEFAULT (sysutcdatetime()),
    [CriadoPor] int NOT NULL,
    [AlteradoEm] datetime2 NOT NULL,
    [AlteradoPor] int NOT NULL,
    CONSTRAINT [PK__Funcoes__3214EC274B9C5F99] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [AcessoFuncoes] (
    [ID] int NOT NULL,
    [AcessoID] int NOT NULL,
    [FuncaoID] int NOT NULL,
    CONSTRAINT [PK_AcessoFuncoes] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [Familias] (
    [Nome] nvarchar(50) NOT NULL,
    [NomeVista] nvarchar(100) NOT NULL,
    [TipoTamanho] nvarchar(10) NOT NULL,
    [Ativo] bit NOT NULL DEFAULT ((1)),
    [Prefixo] nvarchar(2) NOT NULL,
    CONSTRAINT [PK_Familias] PRIMARY KEY ([Nome]),
    CONSTRAINT [CK_TipoTamanho] CHECK ([TipoTamanho]='Numero' OR [TipoTamanho]='Letra')
);
GO

CREATE TABLE [Funcionarios] (
    [Nr] int NOT NULL,
    [Nome] nvarchar(100) NOT NULL,
    [FuncaoID] int NOT NULL,
    [EstabID] int NOT NULL,
    CONSTRAINT [PK__Funciona__3214D4AD70BFF87C] PRIMARY KEY ([Nr])
);
GO

CREATE TABLE [Login] (
    [Nr] int NOT NULL,
    [Password] nvarchar(MAX) NOT NULL
);
GO

CREATE TABLE [FuncionarioTamanhos] (
    [Nr] int NOT NULL,
    [Familia] nvarchar(50) NOT NULL,
    [Tamanho] nvarchar(10) NOT NULL,
    CONSTRAINT [PK_FuncionarioTamanhos] PRIMARY KEY ([Nr],[Familia])
);
GO

CREATE TABLE [EPI] (
    [Codigo] int NOT NULL,
    [Familia] nvarchar(20) NOT NULL,
    [Modelo] nvarchar(100) NOT NULL,
    [Tamanho] nvarchar(20) NOT NULL,
    [CorID] int NOT NULL,
    [AcessoID] int NOT NULL,
    [Preco] decimal(10,2) NOT NULL DEFAULT ((0)),
    [Ativo] bit NOT NULL DEFAULT ((1)),
    CONSTRAINT [PK_EPI] PRIMARY KEY ([Codigo])
);
GO

CREATE TABLE [Stock] (
    [ID] int NOT NULL,
    [Codigo] int NOT NULL,
    [Estado] int NOT NULL,
    [Quantidade] int NOT NULL,
    CONSTRAINT [PK_Stock_1] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [Dispositivos] (
    [IDDispositivo] int NOT NULL,
    [NomePC] nvarchar(100) NOT NULL,
    [UltimoLogin] datetime NOT NULL,
    CONSTRAINT [PK__Disposit__0793151BBC2B444E] PRIMARY KEY ([IDDispositivo])
);
GO

CREATE TABLE [FuncionarioDispositivo] (
    [IDFuncionario] int NOT NULL,
    [IDDispositivo] int NOT NULL,
    [RecebeNotificacoes] bit NOT NULL DEFAULT ((0)),
    [DataDefinicao] datetime NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_FuncionarioDispositivo] PRIMARY KEY ([IDFuncionario],[IDDispositivo])
);
GO

CREATE TABLE [PedidoRegistos] (
    [ID] int NOT NULL,
    [Data] date NOT NULL,
    [NrFunc] int NOT NULL,
    [Estado] nvarchar(20) NOT NULL DEFAULT ('Pendente'),
    [AprovadoPor] int NOT NULL,
    [EntregadoPor] int NOT NULL,
    [CaminhoPDF] nvarchar(500) NOT NULL,
    [Notas] nvarchar(MAX) NOT NULL,
    [CriacaoData] datetime NOT NULL DEFAULT (getdate()),
    [CriadoPor] int NOT NULL,
    [AlteracaoData] datetime NOT NULL,
    [AlteradoPor] int NOT NULL,
    CONSTRAINT [PK__PedidoRe__3214EC27B89AF0E7] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [PedidoPacote] (
    [ID] int NOT NULL,
    [IDPedidoRegisto] int NOT NULL,
    [CodigoEPI] int NOT NULL,
    [Quantidade] int NOT NULL,
    [IDStock] int NOT NULL,
    [TipoMovimento] char(1) NOT NULL DEFAULT ('P'),
    CONSTRAINT [PK__PedidoPa__3214EC27D463FFBB] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [AuditLog] (
    [ID] int NOT NULL,
    [NrFunc] int NOT NULL,
    [DataAlteracao] datetime2 NOT NULL DEFAULT (sysutcdatetime()),
    [AlteradoPor] int NOT NULL,
    [Acao] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_AuditLog] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [Definicoes] (
    [ID] int NOT NULL,
    [Chave] nvarchar(100) NOT NULL,
    [Valor] nvarchar(500) NOT NULL,
    [Tipo] nvarchar(20) NOT NULL,
    [DataAlteracao] datetime NOT NULL,
    [AlteradoPor] int NOT NULL,
    CONSTRAINT [PK__Definico__3214EC275841807F] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [QueriesSalvas] (
    [ID] int NOT NULL,
    [Nome] nvarchar(20) NOT NULL,
    [ConteudoSQL] nvarchar(MAX) NOT NULL,
    [ConfigFiltros] varchar(-1) NOT NULL,
    CONSTRAINT [PK_Query] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [RegrasFamilia] (
    [ID] int NOT NULL,
    [PalavraChave] nvarchar(100) NOT NULL,
    [FamiliaDestino] nvarchar(100) NOT NULL,
    CONSTRAINT [PK__RegrasFa__3214EC2759E19ED5] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [RegrasFuncao] (
    [ID] int NOT NULL,
    [PalavraChave] nvarchar(100) NOT NULL,
    [FuncaoDestino] nvarchar(100) NOT NULL,
    CONSTRAINT [PK__RegrasFu__3214EC2709835508] PRIMARY KEY ([ID])
);
GO

-- ==============================================================
-- PARTE 2: FOREIGN KEYS
-- ==============================================================

ALTER TABLE [AcessoFuncoes] ADD CONSTRAINT [FK_AcessoFuncoes_Acessos] FOREIGN KEY ([AcessoID]) REFERENCES [Acessos]([ID]);
GO
ALTER TABLE [AcessoFuncoes] ADD CONSTRAINT [AcessoFuncoesID_FuncoesID] FOREIGN KEY ([FuncaoID]) REFERENCES [Funcoes]([ID]);
GO
ALTER TABLE [Definicoes] ADD CONSTRAINT [FK_Definicoes_Funcionarios] FOREIGN KEY ([AlteradoPor]) REFERENCES [Funcionarios]([Nr]);
GO
ALTER TABLE [EPI] ADD CONSTRAINT [FK_EPI_Acessos] FOREIGN KEY ([AcessoID]) REFERENCES [Acessos]([ID]);
GO
ALTER TABLE [EPI] ADD CONSTRAINT [FK_EPI_Cor] FOREIGN KEY ([CorID]) REFERENCES [Cor]([ID]);
GO
ALTER TABLE [FuncionarioDispositivo] ADD CONSTRAINT [FK_FuncDispositivo_Dispositivos] FOREIGN KEY ([IDDispositivo]) REFERENCES [Dispositivos]([IDDispositivo]);
GO
ALTER TABLE [Funcionarios] ADD CONSTRAINT [FuncoesID_FuncionariosFuncoes] FOREIGN KEY ([FuncaoID]) REFERENCES [Funcoes]([ID]);
GO
ALTER TABLE [Funcionarios] ADD CONSTRAINT [FK_Funcionarios_Estab] FOREIGN KEY ([EstabID]) REFERENCES [Estabelecimentos]([ID]);
GO
ALTER TABLE [FuncionarioTamanhos] ADD CONSTRAINT [FK_FuncionarioTamanhos_Nr] FOREIGN KEY ([Nr]) REFERENCES [Funcionarios]([Nr]);
GO
ALTER TABLE [Login] ADD CONSTRAINT [FK_LogIn_Funcionarios1] FOREIGN KEY ([Nr]) REFERENCES [Funcionarios]([Nr]);
GO
ALTER TABLE [PedidoPacote] ADD CONSTRAINT [FK_PedidoPacote_PedidoRegistos] FOREIGN KEY ([IDPedidoRegisto]) REFERENCES [PedidoRegistos]([ID]);
GO
ALTER TABLE [PedidoPacote] ADD CONSTRAINT [FK_PedidoPacote_Stock] FOREIGN KEY ([IDStock]) REFERENCES [Stock]([ID]);
GO
ALTER TABLE [PedidoRegistos] ADD CONSTRAINT [FK_PedidoRegistos_AlteradoPor] FOREIGN KEY ([AlteradoPor]) REFERENCES [Funcionarios]([Nr]);
GO
ALTER TABLE [PedidoRegistos] ADD CONSTRAINT [FK_PedidoRegistos_CriadoPor] FOREIGN KEY ([CriadoPor]) REFERENCES [Funcionarios]([Nr]);
GO
ALTER TABLE [PedidoRegistos] ADD CONSTRAINT [FK_PedidoRegistos_Funcionarios] FOREIGN KEY ([NrFunc]) REFERENCES [Funcionarios]([Nr]);
GO
ALTER TABLE [Stock] ADD CONSTRAINT [FK_Stock_EPI] FOREIGN KEY ([Codigo]) REFERENCES [EPI]([Codigo]);
GO
ALTER TABLE [Stock] ADD CONSTRAINT [FK_Stock_Estado] FOREIGN KEY ([Estado]) REFERENCES [Estado]([ID]);
GO

ALTER TABLE [Definicoes] ADD CONSTRAINT [UQ_Definicoes_Chave] UNIQUE ([Chave]); GO
ALTER TABLE [Funcoes]    ADD CONSTRAINT [UQ_Funcoes_Nome]      UNIQUE ([Nome]);  GO
ALTER TABLE [Familias]   ADD CONSTRAINT [UQ_Familias_Prefixo]  UNIQUE ([Prefixo]); GO

-- ==============================================================
-- PARTE 3: STORED PROCEDURES
-- ==============================================================

-- GetFuncionarioInfo

-- ============================================================
-- PEPIDI -- FIX: Atualizar Stored Procedures após renames
-- (continuação do schema_normalizar.sql, FASE 5 que falhou
--  porque o PRINT anterior impediu o ALTER PROCEDURE de ser
--  a primeira instrução do batch)
-- ============================================================

-- -----------------------------------------------------------------
-- GetFuncionarioInfo: referencia Funcionarios.Funcao → agora FuncaoID
-- Nota: o parâmetro de saída @Funcao mantém o nome (interface pública
-- não muda) — apenas a coluna interna é atualizada.
-- -----------------------------------------------------------------
CREATE PROCEDURE [dbo].[GetFuncionarioInfo]
    @NrFunc  INT,
    @Nome    NVARCHAR(100) OUTPUT,
    @Funcao  NVARCHAR(100) OUTPUT
AS
BEGIN
    SELECT @Nome = Nome, @Funcao = FuncaoID
    FROM Funcionarios
    WHERE Nr = @NrFunc;
END


GO

-- sp_AlterarPassword

-- -----------------------------------------------------------------
-- sp_AlterarPassword: referencia LogIn → agora Login
-- -----------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_AlterarPassword]
    @NrFunc             INT,
    @NovaPasswordHash   NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Login WHERE Nr = @NrFunc)
    BEGIN
        UPDATE Login
        SET Password = @NovaPasswordHash
        WHERE Nr = @NrFunc
    END
    ELSE
    BEGIN
        IF EXISTS (SELECT 1 FROM Funcionarios WHERE Nr = @NrFunc)
        BEGIN
            INSERT INTO Login (Nr, Password) VALUES (@NrFunc, @NovaPasswordHash)
        END
        ELSE
        BEGIN
            RAISERROR('Funcionário não existe.', 16, 1);
        END
    END
END


GO

-- sp_AtualizarDefinicao
CREATE PROCEDURE [dbo].[sp_AtualizarDefinicao]
    @Chave NVARCHAR(100),
    @Valor NVARCHAR(500),
    @AlteradoPor INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Definicoes WHERE Chave = @Chave)
    BEGIN
        UPDATE Definicoes
        SET Valor = @Valor,
            DataAlteracao = GETDATE(),
            AlteradoPor = @AlteradoPor
        WHERE Chave = @Chave;
    END
    ELSE
    BEGIN
        INSERT INTO Definicoes (Chave, Valor, Tipo, DataAlteracao, AlteradoPor)
        VALUES (@Chave, @Valor, NULL, GETDATE(), @AlteradoPor);
    END
END;

GO

-- sp_AtualizarPermissoesFuncao

-- ================================================================
-- fix_podeVerHistorico.sql
-- 1. Actualiza sp_AtualizarPermissoesFuncao para incluir PodeVerHistorico
-- 2. Corrige dados: activa PodeVerHistorico para RH e Encarregado
-- ================================================================

-- 1. SP actualizada
CREATE PROCEDURE [dbo].[sp_AtualizarPermissoesFuncao]
    @ID                   INT,
    @PodeVerStock         BIT,
    @PodeInserirStock     BIT,
    @PodeCriarStock       BIT,
    @PodeVerHistorico     BIT,
    @PodeEditarFunc       BIT,
    @PodeSubmeter         BIT,
    @PodeAprovar          BIT,
    @PodeEntregar         BIT,
    @PodeCriarFuncoes     BIT,
    @PodeAlterarDefinicoes BIT,
    @PodeVerUsados        BIT,
    @AlteradoPor          INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Funcoes SET
        PodeVerStock          = @PodeVerStock,
        PodeInserirStock      = @PodeInserirStock,
        PodeCriarStock        = @PodeCriarStock,
        PodeVerHistorico      = @PodeVerHistorico,
        PodeEditarFunc        = @PodeEditarFunc,
        PodeSubmeter          = @PodeSubmeter,
        PodeAprovar           = @PodeAprovar,
        PodeEntregar          = @PodeEntregar,
        PodeCriarFuncoes      = @PodeCriarFuncoes,
        PodeAlterarDefinicoes = @PodeAlterarDefinicoes,
        PodeVerUsados         = @PodeVerUsados,
        AlteradoEm            = SYSDATETIME(),
        AlteradoPor           = @AlteradoPor
    WHERE ID = @ID;
END

GO

-- sp_AtualizarQuantidadePedidoPacote

-- ----------------------------------------------------------------
-- 6. sp_AtualizarQuantidadePedidoPacote
--    Bug: coluna IDEPI não existe — é CodigoEPI.
--    Parâmetro mantido como INT para compatibilidade com o C# existente
--    (se Codigo for VARCHAR, alterar para NVARCHAR aqui e no C#).
-- ----------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_AtualizarQuantidadePedidoPacote]
    @IDPedido       INT,
    @IDEPI          INT,
    @NovaQuantidade INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE PedidoPacote
    SET Quantidade = @NovaQuantidade
    WHERE IDPedidoRegisto = @IDPedido AND CodigoEPI = @IDEPI;
END


GO

-- sp_BuscaInfoFuncAtivo
CREATE PROCEDURE [dbo].[sp_BuscaInfoFuncAtivo] @Nr INT AS BEGIN SELECT f.Nome, fun.Nome AS Funcao FROM Funcionarios f JOIN Funcoes fun ON fun.ID = f.FuncaoID WHERE f.Nr = @Nr; END
GO

-- sp_CarregarPedidosPorEstado

-- ------------------------------------------------------------
-- sp_CarregarPedidosPorEstado  →  Funcao, Aprovacao, Entrega, PDF
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_CarregarPedidosPorEstado]
    @Estado NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Estado = 'Finalizado'
    BEGIN
        SELECT
            R.ID,
            R.Data,
            R.NrFunc,
            F.Nome AS NomeFunc,
            Fu.Nome AS Funcao,
            Fu.CorHex,
            R.Estado,
            FA.Nome AS NomeAprovador,
            FE.Nome AS NomeEntrega,
            R.Notas,
            R.CaminhoPDF AS PDF
        FROM PedidoRegistos R
        JOIN Funcionarios F ON F.Nr = R.NrFunc
        JOIN Funcoes Fu ON Fu.ID = F.FuncaoID
        LEFT JOIN Funcionarios FA ON FA.Nr = R.AprovadoPor
        LEFT JOIN Funcionarios FE ON FE.Nr = R.EntregadoPor
        WHERE R.Estado IN ('Finalizado', 'Rejeitado')
        ORDER BY R.Data DESC
    END
    ELSE
    BEGIN
        SELECT
            R.ID,
            R.Data,
            R.NrFunc,
            F.Nome AS NomeFunc,
            Fu.Nome AS Funcao,
            Fu.CorHex,
            R.Estado,
            FA.Nome AS NomeAprovador,
            FE.Nome AS NomeEntrega,
            R.CaminhoPDF AS PDF
        FROM PedidoRegistos R
        JOIN Funcionarios F ON F.Nr = R.NrFunc
        JOIN Funcoes Fu ON Fu.ID = F.FuncaoID
        LEFT JOIN Funcionarios FA ON FA.Nr = R.AprovadoPor
        LEFT JOIN Funcionarios FE ON FE.Nr = R.EntregadoPor
        WHERE R.Estado = @Estado
        ORDER BY R.Data DESC
    END
END


GO

-- sp_ConsumoPorFuncionario
-- ================================================================
-- sps_fix_bugs.sql
-- Corrige 6 SPs que falharam em sps_atualizar.sql por bugs
-- pré-existentes agora apanhados pelo SQL Server em tempo de ALTER.
--
-- EXECUTAR DEPOIS de sps_atualizar.sql (ou em substituição dos
-- blocos que deram erro).
-- ================================================================

-- ----------------------------------------------------------------
-- 1. sp_ConsumoPorFuncionario
--    Bug: E.ID (não existe — é E.Codigo) e P.IDEPI (não existe — é P.CodigoEPI)
-- ----------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_ConsumoPorFuncionario]
    @NrFunc INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        E.Modelo,
        SUM(P.Quantidade) AS TotalConsumido
    FROM PedidoRegistos R
    JOIN PedidoPacote P ON P.IDPedidoRegisto = R.ID
    JOIN EPI E ON E.Codigo = P.CodigoEPI
    WHERE R.NrFunc = @NrFunc AND R.Estado = 'Finalizado'
    GROUP BY E.Modelo
    ORDER BY TotalConsumido DESC;
END


GO

-- sp_ConsumosFiltrados

-- ------------------------------------------------------------
-- sp_ConsumosFiltrados  →  IDPedReg, Funcao
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_ConsumosFiltrados]
(
    @NrFunc       INT            = NULL,
    @Funcoes      NVARCHAR(MAX)  = NULL,
    @Familias     NVARCHAR(MAX)  = NULL,
    @Modelos      NVARCHAR(MAX)  = NULL,
    @Tamanhos     NVARCHAR(MAX)  = NULL,
    @DataInicio   DATE           = NULL,
    @DataFim      DATE           = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        pr.Data,
        f.Nr                    AS NrFunc,
        f.Nome                  AS NomeFuncionario,
        fu.Nome                 AS Funcao,
        e.Familia,

        CASE
            WHEN s.Estado = 2 THEN e.Modelo + ' [USADO]'
            ELSE e.Modelo
        END AS Modelo,

        e.Tamanho,
        pp.Quantidade,

        CASE
            WHEN s.Estado = 2 THEN 0
            ELSE ISNULL(e.Preco, 0)
        END AS PrecoUnitario,

        CASE
            WHEN s.Estado = 2 THEN 0
            ELSE (pp.Quantidade * ISNULL(e.Preco, 0))
        END AS TotalGasto

    FROM PedidoRegistos pr
    INNER JOIN PedidoPacote pp   ON pp.IDPedidoRegisto = pr.ID
    INNER JOIN EPI e             ON e.Codigo = pp.CodigoEPI
    LEFT JOIN Stock s            ON s.ID = pp.IDStock

    INNER JOIN Funcionarios f    ON f.Nr = pr.NrFunc
    LEFT  JOIN Funcoes fu        ON fu.ID = f.FuncaoID

    WHERE
        pr.Estado IN ('Aprovado', 'Finalizado', 'Concluido')
        AND pp.Quantidade > 0

        AND (@NrFunc IS NULL OR f.Nr = @NrFunc)

        AND (
            @Funcoes IS NULL
            OR f.FuncaoID IN (SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@Funcoes, ','))
        )
        AND (
            @Familias IS NULL
            OR e.Familia IN (SELECT value FROM STRING_SPLIT(@Familias, ','))
        )
        AND (
            @Modelos IS NULL
            OR e.Modelo IN (SELECT value FROM STRING_SPLIT(@Modelos, ','))
        )
        AND (
            @Tamanhos IS NULL
            OR e.Tamanho IN (SELECT value FROM STRING_SPLIT(@Tamanhos, ','))
        )

        AND (@DataInicio IS NULL OR CAST(pr.Data AS DATE) >= @DataInicio)
        AND (@DataFim    IS NULL OR CAST(pr.Data AS DATE) <= @DataFim)

    ORDER BY
        pr.Data DESC,
        f.Nome,
        e.Modelo,
        e.Tamanho;
END


GO

-- sp_DefinirDispositivoNotificacoes
CREATE PROCEDURE [dbo].[sp_DefinirDispositivoNotificacoes]
    @IDFuncionario INT,
    @NomePC        NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IDDispositivo INT;

    SELECT @IDDispositivo = IDDispositivo
    FROM Dispositivos
    WHERE NomePC = @NomePC;

    IF @IDDispositivo IS NULL
    BEGIN
        RAISERROR('Dispositivo não registado.', 16, 1);
        RETURN;
    END

    -- Desativar todos os dispositivos do funcionário
    UPDATE FuncionarioDispositivo
    SET RecebeNotificacoes = 0
    WHERE IDFuncionario = @IDFuncionario;

    -- Ativar só este
    UPDATE FuncionarioDispositivo
    SET RecebeNotificacoes = 1
    WHERE IDFuncionario = @IDFuncionario
      AND IDDispositivo   = @IDDispositivo;
END

GO

-- sp_DetalhesDaDevolucao

-- ------------------------------------------------------------
-- sp_DetalhesDaDevolucao  →  E.Cor, IDPedReg
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_DetalhesDaDevolucao]
    @IDPedido INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        RP.ID AS IDLinhaDevolucao,
        RP.CodigoEPI AS Codigo,
        E.Modelo + ISNULL(' ' + C.Nome, '') AS ModeloComCor,
        E.Tamanho,
        RP.Quantidade AS QuantidadeDevolvida,
        S.Estado AS EstadoID
    FROM PedidoPacote RP
    INNER JOIN EPI E ON RP.CodigoEPI = E.Codigo
    LEFT JOIN Cor C ON E.CorID = C.ID
    LEFT JOIN Stock S ON RP.IDStock = S.ID
    WHERE RP.IDPedidoRegisto = @IDPedido AND RP.TipoMovimento = 'D'
END


GO

-- sp_DetalhesDoPedido

-- ------------------------------------------------------------
-- sp_DetalhesDoPedido  →  E.Cor, S.Quant, IDPedReg
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_DetalhesDoPedido]
    @IDPedido INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        PP.ID AS IDLinhaPedido,
        PP.CodigoEPI AS Codigo,
        E.Modelo + ISNULL(' ' + C.Nome, '') AS ModeloComCor,
        E.Tamanho,
        PP.Quantidade AS QuantidadePedida,
        PP.IDStock,
        S.Estado AS EstadoID,
        ISNULL(S.Quantidade, 0) AS QtdDisponivelEstadoSeleccionado,
        ISNULL(SN.Quantidade, 0) AS QtdStockNovo,
        ISNULL(SU.Quantidade, 0) AS QtdStockUsado
    FROM PedidoPacote PP
    INNER JOIN EPI E ON PP.CodigoEPI = E.Codigo
    LEFT JOIN Cor C ON E.CorID = C.ID
    LEFT JOIN Stock S ON PP.IDStock = S.ID
    LEFT JOIN Stock SN ON PP.CodigoEPI = SN.Codigo AND SN.Estado = 1
    LEFT JOIN Stock SU ON PP.CodigoEPI = SU.Codigo AND SU.Estado = 2
    WHERE PP.IDPedidoRegisto = @IDPedido AND (PP.TipoMovimento IS NULL OR PP.TipoMovimento = 'P')
END


GO

-- sp_FinalizarPedido
CREATE PROCEDURE [dbo].[sp_FinalizarPedido]
    @ID INT
AS
BEGIN
    UPDATE PedidoRegistos SET Estado = 'Finalizado' WHERE ID = @ID
END

GO

-- sp_GerarConsumosAleatorios

-- ------------------------------------------------------------
-- sp_GerarConsumosAleatorios  →  Aprovacao, Entrega, IDPedReg, S.Quant
-- Nota: variável local @IDPedReg fica intacta (não é coluna).
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_GerarConsumosAleatorios]
(
    @QtdRegistos INT = 3000,
    @DataInicio  DATE = '2025-01-01',
    @DataFim     DATE = NULL,
    @NrCriador   INT  = 1077,
    @Estado      VARCHAR(50) = 'Finalizado'
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @DataFim IS NULL SET @DataFim = CAST(GETDATE() AS DATE);

    IF NOT EXISTS (SELECT 1 FROM Funcionarios WHERE Nr = @NrCriador)
    BEGIN
        PRINT 'Erro: O criador especificado não existe na base de dados.';
        RETURN;
    END

    DECLARE @Funcs TABLE (RowNum INT IDENTITY(1,1), NrFunc INT);
    INSERT INTO @Funcs (NrFunc)
    SELECT Nr FROM Funcionarios;

    DECLARE @QtdFuncs INT = (SELECT COUNT(*) FROM @Funcs);
    IF @QtdFuncs = 0
    BEGIN
        PRINT 'Erro: Não existem funcionários na tabela para associar aos pedidos.';
        RETURN;
    END

    DECLARE @i INT = 1, @randFuncIdx INT, @NrFunc INT;
    DECLARE @CodigoAtual INT, @IDStock INT, @QtdLinha INT, @MaxQuant INT;
    DECLARE @LinhasPorPedido INT, @IDPedReg INT, @DataPedido DATETIME, @DiasIntervalo INT;

    SET @DiasIntervalo = DATEDIFF(DAY, @DataInicio, @DataFim);
    IF @DiasIntervalo < 0 SET @DiasIntervalo = 0;

    WHILE @i <= @QtdRegistos
    BEGIN
        SET @randFuncIdx = (ABS(CHECKSUM(NEWID())) % @QtdFuncs) + 1;
        SELECT @NrFunc = NrFunc FROM @Funcs WHERE RowNum = @randFuncIdx;

        SET @DataPedido = DATEADD(DAY, CASE WHEN @DiasIntervalo = 0 THEN 0 ELSE ABS(CHECKSUM(NEWID())) % (@DiasIntervalo + 1) END, @DataInicio);

        INSERT INTO PedidoRegistos (Data, NrFunc, Estado, AprovadoPor, EntregadoPor, Notas, CriacaoData, CriadoPor, AlteracaoData, AlteradoPor)
        VALUES (@DataPedido, @NrFunc, @Estado, 1, 1, '[TESTE] Consumo Aleatório Automático', GETDATE(), @NrCriador, GETDATE(), @NrCriador);

        SET @IDPedReg = SCOPE_IDENTITY();

        SET @LinhasPorPedido = (ABS(CHECKSUM(NEWID())) % 5) + 1;
        DECLARE @j INT = 1;

        WHILE @j <= @LinhasPorPedido
        BEGIN
            SET @CodigoAtual = NULL;

            SELECT TOP 1
                @CodigoAtual = E.Codigo,
                @IDStock = S.ID,
                @MaxQuant = S.Quantidade
            FROM EPI E
            INNER JOIN Stock S ON E.Codigo = S.Codigo
            WHERE S.Estado = 1 AND S.Quantidade > 0
            ORDER BY NEWID();

            IF @CodigoAtual IS NOT NULL
            BEGIN
                SET @QtdLinha = (ABS(CHECKSUM(NEWID())) % 5) + 1;

                IF @QtdLinha > @MaxQuant
                    SET @QtdLinha = @MaxQuant;

                INSERT INTO PedidoPacote (IDPedidoRegisto, CodigoEPI, Quantidade, IDStock)
                VALUES (@IDPedReg, @CodigoAtual, @QtdLinha, @IDStock);

                UPDATE Stock
                SET Quantidade = Quantidade - @QtdLinha
                WHERE ID = @IDStock;
            END

            SET @j += 1;
        END;

        SET @i += 1;
    END;
END;


GO

-- sp_GetFuncionarioDetails

-- ------------------------------------------------------------
-- sp_GetFuncionarioDetails  →  F.Funcao (alias para preservar C#)
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_GetFuncionarioDetails]
    @IDPedido INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT F.Nr, F.Nome, F.FuncaoID AS Funcao
    FROM Funcionarios F
    JOIN PedidoRegistos PR ON F.Nr = PR.NrFunc
    WHERE PR.ID = @IDPedido;
END


GO

-- sp_GetNomeFunc

CREATE   PROCEDURE [dbo].[sp_GetNomeFunc]
    @IDPedReg INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        f.Nome
    FROM PedidoRegistos pr
    INNER JOIN Funcionarios f
        ON f.Nr = pr.NrFunc
    WHERE pr.ID = @IDPedReg;
END

GO

-- sp_GetTamanhosPorModelo

-- ------------------------------------------------------------
-- sp_GetTamanhosPorModelo  →  Cor → CorID
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_GetTamanhosPorModelo]
    @Modelo VARCHAR(100),
    @Cor    VARCHAR(20)
AS
BEGIN
    SELECT DISTINCT Tamanho
    FROM EPI
    WHERE Modelo = @Modelo AND CorID = @Cor AND Ativo = 1
END


GO

-- sp_HistoricoPorFuncionario

-- ----------------------------------------------------------------
-- 2. sp_HistoricoPorFuncionario
--    Bug: E.ID → E.Codigo  /  P.IDEPI → P.CodigoEPI
-- ----------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_HistoricoPorFuncionario]
    @NrFunc INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        R.Data,
        E.Modelo,
        E.Tamanho,
        SUM(P.Quantidade) AS QuantidadeTotal
    FROM PedidoRegistos R
    JOIN PedidoPacote P ON P.IDPedidoRegisto = R.ID
    JOIN EPI E ON E.Codigo = P.CodigoEPI
    WHERE R.NrFunc = @NrFunc AND R.Estado = 'Finalizado'
    GROUP BY R.Data, E.Modelo, E.Tamanho
    ORDER BY R.Data DESC, E.Modelo, E.Tamanho;
END


GO

-- sp_InserirFuncao

CREATE PROCEDURE [dbo].[sp_InserirFuncao]
    @ID          INT = NULL,      -- Se NULL, cria novo. Se preenchido, edita.
    @Nome        NVARCHAR(100),
    @CorHex      NVARCHAR(20),    -- NOVO: Código da cor (ex: #FF0000)
    @CriadoPor   INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Verifica se é para ATUALIZAR (tem ID e o ID existe)
    IF @ID IS NOT NULL AND EXISTS (SELECT 1 FROM Funcoes WHERE ID = @ID)
    BEGIN
        UPDATE Funcoes
        SET Nome = @Nome,
            CorHex = @CorHex      -- Atualiza a cor
        WHERE ID = @ID;

        SELECT @ID AS NovoID;
    END
    -- Senão, é para INSERIR
    ELSE
    BEGIN
        INSERT INTO Funcoes (Nome, CriadoPor, CorHex)
        VALUES (@Nome, @CriadoPor, @CorHex); -- Insere a cor

        SELECT CAST(SCOPE_IDENTITY() AS INT) AS NovoID;
    END
END;

GO

-- sp_ListarEPINaoUnicos

-- ----------------------------------------------------------------
-- 3. sp_ListarEPINaoUnicos
--    Bug: E.Quantidade — a tabela EPI não tem esta coluna.
--    Removida do SELECT (a coluna não existe e não há equivalente direto).
-- ----------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_ListarEPINaoUnicos]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        E.Familia,
        E.Modelo,
        E.Tamanho
    FROM EPI E
    WHERE E.AcessoID IN (
        SELECT AF.AcessoID
        FROM AcessoFuncoes AF
        GROUP BY AF.AcessoID
        HAVING COUNT(DISTINCT AF.FuncaoID) > 1
    )
END


GO

-- sp_ListarEPIporFuncao

-- ----------------------------------------------------------------
-- 4. sp_ListarEPIporFuncao
--    Bug: E.Quantidade — mesma situação. Removida.
-- ----------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_ListarEPIporFuncao]
    @Funcao NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        E.Familia,
        E.Modelo,
        E.Tamanho
    FROM EPI E
    INNER JOIN Acessos A ON A.ID = E.AcessoID
    WHERE EXISTS (
        SELECT 1
        FROM AcessoFuncoes AF
        INNER JOIN Funcoes F ON F.ID = AF.FuncaoID
        WHERE AF.AcessoID = A.ID
          AND F.Nome = @Funcao
    )
    AND NOT EXISTS (
        SELECT 1
        FROM AcessoFuncoes AF2
        INNER JOIN Funcoes F2 ON F2.ID = AF2.FuncaoID
        WHERE AF2.AcessoID = A.ID
          AND F2.Nome <> @Funcao
    )
END


GO

-- sp_ListarFuncionariosComFuncoes

-- ------------------------------------------------------------
-- sp_ListarFuncionariosComFuncoes
-- Remove colunas de tamanho e DtAdmiss que já não existem
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_ListarFuncionariosComFuncoes]
AS
BEGIN
    SELECT
        f.Nr,
        f.Nome,
        fun.Nome  AS Funcao,
        f.EstabID AS Estab
    FROM Funcionarios f
    JOIN Funcoes fun ON fun.ID = f.FuncaoID
    WHERE f.Nr <> 0
END

GO

-- sp_ListarNomesDasQueries

-- ------------------------------------------------------------
-- sp_ListarNomesDasQueries  →  Query → QueriesSalvas
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_ListarNomesDasQueries]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ID, Nome
    FROM QueriesSalvas
    ORDER BY ID;
END


GO

-- sp_ObterDefinicao
CREATE PROCEDURE [dbo].[sp_ObterDefinicao]
    @Chave NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Valor
    FROM Definicoes
    WHERE Chave = @Chave;
END;

GO

-- sp_ObterFuncionarioPorNr

-- ------------------------------------------------------------
-- sp_ObterFuncionarioPorNr
-- Remove colunas de tamanho — C# lê tamanhos numa query separada
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_ObterFuncionarioPorNr]
    @Nr INT
AS
BEGIN
    SELECT
        F.Nr,
        F.Nome,
        F.FuncaoID AS Funcao,
        FN.Nome    AS NomeFuncao,
        F.EstabID  AS Estab,
        Es.Nome    AS NomeEstab
    FROM Funcionarios F
    INNER JOIN Funcoes FN         ON F.FuncaoID = FN.ID
    INNER JOIN Estabelecimentos Es ON F.EstabID  = Es.ID
    WHERE F.Nr = @Nr;
END

GO

-- sp_ObterFuncionarioPorPedido

-- ------------------------------------------------------------
-- sp_ObterFuncionarioPorPedido  →  F.Funcao → F.FuncaoID
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_ObterFuncionarioPorPedido]
    @IDPedido INT
AS
BEGIN
    SELECT
        F.Nome,
        F.Nr,
        FN.Nome AS Funcao
    FROM PedidoRegistos P
    INNER JOIN Funcionarios F ON P.NrFunc = F.Nr
    LEFT JOIN Funcoes FN ON F.FuncaoID = FN.ID
    WHERE P.ID = @IDPedido
END


GO

-- sp_ProcurarFuncionarios

-- ------------------------------------------------------------
-- sp_ProcurarFuncionarios  →  f.Funcao, f.Estab, JOIN Estab, E.Estab
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_ProcurarFuncionarios]
    @Termo NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        f.Nr,
        f.Nome,
        fun.Nome AS Funcao,
        fun.CorHex,
        E.Nome AS Estab
    FROM Funcionarios f
    JOIN Funcoes fun ON fun.ID = f.FuncaoID
    JOIN Estabelecimentos E ON E.ID = f.EstabID
    WHERE
        f.Nome COLLATE Latin1_General_CI_AI LIKE '%' + @Termo + '%'

        OR

        CAST(f.Nr AS NVARCHAR(20)) LIKE '%' + @Termo + '%'

        OR

        fun.Nome COLLATE Latin1_General_CI_AI LIKE '%' + @Termo + '%'

        OR

        E.Nome COLLATE Latin1_General_CI_AI LIKE '%' + @Termo + '%'
END


GO

-- sp_ProdutosConsumidosPorFuncionario

-- ------------------------------------------------------------
-- sp_ProdutosConsumidosPorFuncionario  →  IDPedReg, E.Cor
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_ProdutosConsumidosPorFuncionario]
    @NrFunc INT
AS
BEGIN
    SELECT DISTINCT
        E.Codigo,
        E.Familia,
        E.Modelo,
        E.Tamanho,
        E.CorID AS Cor
    FROM PedidoPacote PP
    INNER JOIN PedidoRegistos PR ON PP.IDPedidoRegisto = PR.ID
    INNER JOIN EPI E ON PP.CodigoEPI = E.Codigo
    WHERE PR.NrFunc = @NrFunc AND PR.Estado = 'Finalizado'
END


GO

-- sp_RegistaLoginDispositivo
CREATE PROCEDURE [dbo].[sp_RegistaLoginDispositivo]
    @IDFuncionario INT,
    @NomePC        NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IDDispositivo INT;

    SELECT @IDDispositivo = IDDispositivo
    FROM Dispositivos
    WHERE NomePC = @NomePC;

    IF @IDDispositivo IS NULL
    BEGIN
        INSERT INTO Dispositivos (NomePC, UltimoLogin)
        VALUES (@NomePC, GETDATE());

        SET @IDDispositivo = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE Dispositivos
        SET UltimoLogin = GETDATE()
        WHERE IDDispositivo = @IDDispositivo;
    END

    -- Garante que pelo menos existe o registo de ligação funcionário–dispositivo
    IF NOT EXISTS (
        SELECT 1 FROM FuncionarioDispositivo
        WHERE IDFuncionario = @IDFuncionario AND IDDispositivo = @IDDispositivo
    )
    BEGIN
        INSERT INTO FuncionarioDispositivo (IDFuncionario, IDDispositivo, RecebeNotificacoes)
        VALUES (@IDFuncionario, @IDDispositivo, 0);  -- por defeito NÃO recebe notificações
    END
END

GO

-- sp_RegistarLogin

-- ------------------------------------------------------------
-- sp_RegistarLogin  →  LogIn → Login
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_RegistarLogin]
    @Nr VARCHAR(50),
    @PasswordHash VARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Login WHERE Nr = @Nr)
    BEGIN
        UPDATE Login
        SET Password = @PasswordHash
        WHERE Nr = @Nr;
    END
    ELSE
    BEGIN
        INSERT INTO Login (Nr, Password)
        VALUES (@Nr, @PasswordHash);
    END
END


GO

-- sp_ResetPEPIDI
CREATE   PROCEDURE sp_ResetPEPIDI
AS
BEGIN
    SET NOCOUNT ON;

    PRINT '=== sp_ResetPEPIDI: inicio ===';
    PRINT 'Mantido: Funcoes (todas) | Funcionario 1077 | Login 1077 | FuncionarioTamanhos 1077 | dados de referencia.';
    PRINT 'Apagado: todos os outros funcionarios, login, EPI, Stock, Pedidos, AuditLog, Dispositivos.';

    -- 1. Desativar triggers (SESSION_CONTEXT nao esta ativo em sessoes manuais)
    DISABLE TRIGGER ALL ON Funcionarios;
    DISABLE TRIGGER ALL ON FuncionarioTamanhos;
    DISABLE TRIGGER ALL ON Login;
    DISABLE TRIGGER ALL ON PedidoRegistos;

    -- 2. Folhas: nada depende destes
    PRINT 'A limpar AuditLog...';
    DELETE FROM AuditLog;

    PRINT 'A limpar FuncionarioDispositivo...';
    DELETE FROM FuncionarioDispositivo;

    -- 3. PedidoPacote (depende de PedidoRegistos + Stock)
    PRINT 'A limpar PedidoPacote...';
    DELETE FROM PedidoPacote;

    -- 4. PedidoRegistos (depende de Funcionarios)
    PRINT 'A limpar PedidoRegistos...';
    DELETE FROM PedidoRegistos;

    -- 5. FuncionarioTamanhos: manter 1077
    PRINT 'A limpar FuncionarioTamanhos (exceto 1077)...';
    DELETE FROM FuncionarioTamanhos WHERE Nr <> 1077;

    -- 6. Login: manter 1077
    PRINT 'A limpar Login (exceto 1077)...';
    DELETE FROM Login WHERE Nr <> 1077;

    -- 7. Stock (depende de EPI e Estado)
    PRINT 'A limpar Stock...';
    DELETE FROM Stock;

    -- 8. EPI (depende de Acessos e Cor)
    PRINT 'A limpar EPI...';
    DELETE FROM EPI;

    -- 9. Funcionarios: manter 1077
    PRINT 'A limpar Funcionarios (exceto 1077)...';
    DELETE FROM Funcionarios WHERE Nr <> 1077;

    -- 10. Sem dependentes
    PRINT 'A limpar Dispositivos...';
    DELETE FROM Dispositivos;

    PRINT 'A limpar RegrasFuncao...';
    DELETE FROM RegrasFuncao;

    -- 11. RESEED das colunas IDENTITY afetadas
    PRINT 'A reiniciar IDENTITY...';
    DBCC CHECKIDENT ('AuditLog',       RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('PedidoPacote',   RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('PedidoRegistos', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('Dispositivos',   RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('Stock',          RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('RegrasFuncao',   RESEED, 0) WITH NO_INFOMSGS;

    -- 12. Reativar triggers
    ENABLE TRIGGER ALL ON Funcionarios;
    ENABLE TRIGGER ALL ON FuncionarioTamanhos;
    ENABLE TRIGGER ALL ON Login;
    ENABLE TRIGGER ALL ON PedidoRegistos;

    PRINT '=== sp_ResetPEPIDI: concluido. ===';
    PRINT 'Passo seguinte: importar funcionarios e EPI/Stock pela app.';
END;
GO

-- sp_RoupaPorFuncionario

-- ============================================================
-- Atualização das SPs para FuncionarioTamanhos (branch Familias)
-- ============================================================

-- ------------------------------------------------------------
-- sp_RoupaPorFuncionario
-- Antes: lia TShirt/Casaco/etc. de Funcionarios como variáveis
-- Depois: usa FuncionarioTamanhos com fallback para Nr=0
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_RoupaPorFuncionario]
    @NrFunc INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FuncaoID INT;
    SELECT @FuncaoID = FuncaoID FROM Funcionarios WHERE Nr = @NrFunc;

    SELECT
        E.Codigo,
        E.Familia,
        E.Modelo,
        E.Tamanho,
        E.CorID AS Cor,
        ISNULL(SUM(S.Quantidade), 0) AS Quantidade
    FROM AcessoFuncoes AF
    INNER JOIN EPI E ON E.AcessoID = AF.AcessoID
    LEFT JOIN Stock S ON E.Codigo = S.Codigo AND S.Estado = 1
    WHERE AF.FuncaoID = @FuncaoID
      AND (
            -- Sem default global para esta família → mostrar todos os tamanhos
            -- (funcionário novo ou família ainda sem configuração de tamanhos)
            NOT EXISTS (
                SELECT 1 FROM FuncionarioTamanhos WHERE Nr = 0 AND Familia = E.Familia
            )
            OR
            -- Default global existe → filtrar pelo tamanho do funcionário ou pelo default
            EXISTS (
                SELECT 1
                FROM FuncionarioTamanhos d
                LEFT JOIN FuncionarioTamanhos f
                    ON f.Nr = @NrFunc AND f.Familia = d.Familia
                WHERE d.Nr = 0
                  AND d.Familia = E.Familia
                  AND ISNULL(f.Tamanho, d.Tamanho) = E.Tamanho
            )
      )
      AND E.Ativo = 1
    GROUP BY E.Codigo, E.Familia, E.Modelo, E.Tamanho, E.CorID;
END

GO

-- sp_UPSERT_FUNC

-- ------------------------------------------------------------
-- sp_UPSERT_FUNC
-- Remove parâmetros e colunas de tamanho
-- C# faz UPSERT em FuncionarioTamanhos separadamente
-- ------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_UPSERT_FUNC]
    @Modo     CHAR(1),
    @Nr       INT,
    @Nome     NVARCHAR(150) = NULL,
    @FuncaoId INT           = NULL,
    @EstabId  INT           = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Modo NOT IN ('I', 'U')
    BEGIN
        RAISERROR ('Modo inválido. Use ''I'' para Inserir ou ''U'' para Atualizar.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        IF @Modo = 'I'
        BEGIN
            IF EXISTS (SELECT 1 FROM dbo.Funcionarios WHERE Nr = @Nr)
            BEGIN
                RAISERROR ('Erro: Já existe um funcionário registado com o número %d.', 16, 1, @Nr);
                RETURN;
            END

            IF @Nome IS NULL OR @FuncaoId IS NULL
            BEGIN
                RAISERROR ('Nome e Função são obrigatórios para novos registos.', 16, 1);
                RETURN;
            END

            INSERT INTO dbo.Funcionarios (Nr, Nome, FuncaoID, EstabID)
            VALUES (@Nr, @Nome, @FuncaoId, @EstabId);

            SELECT 'inserted' AS Acao, @@ROWCOUNT AS RowsAffected;
        END

        ELSE IF @Modo = 'U'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.Funcionarios WHERE Nr = @Nr)
            BEGIN
                RAISERROR ('Erro: Funcionário %d não encontrado para atualização.', 16, 1, @Nr);
                RETURN;
            END

            UPDATE dbo.Funcionarios
            SET Nome     = ISNULL(@Nome,     Nome),
                FuncaoID = ISNULL(@FuncaoId, FuncaoID),
                EstabID  = ISNULL(@EstabId,  EstabID)
            WHERE Nr = @Nr;

            SELECT 'updated' AS Acao, @@ROWCOUNT AS RowsAffected;
        END
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR (@ErrorMessage, 16, 1);
    END CATCH
END

GO

-- sp_UpsertDefinicao
CREATE PROCEDURE [dbo].[sp_UpsertDefinicao]
    @Chave NVARCHAR(100),
    @Valor NVARCHAR(MAX),
    @Tipo NVARCHAR(50),
    @AlteradoPor NVARCHAR(100) -- Pode ser INT se guardares o ID do Funcionario, ajusta se necessário
AS
BEGIN
    -- Evita mensagens de contagem de linhas para melhorar a performance
    SET NOCOUNT ON;

    -- Verifica se a chave de configuração já existe na tabela
    IF EXISTS (SELECT 1 FROM Definicoes WHERE Chave = @Chave)
    BEGIN
        -- Se já existe, atualiza os dados
        UPDATE Definicoes
        SET Valor = @Valor,
            Tipo = @Tipo,
            DataAlteracao = GETDATE(), -- O SQL assume a data e hora do momento exato
            AlteradoPor = @AlteradoPor
        WHERE Chave = @Chave;
    END
    ELSE
    BEGIN
        -- Se não existe, insere um novo registo
        INSERT INTO Definicoes (Chave, Valor, Tipo, DataAlteracao, AlteradoPor)
        VALUES (@Chave, @Valor, @Tipo, GETDATE(), @AlteradoPor);
    END
END

GO

-- ==============================================================
-- PARTE 4: TRIGGERS
-- ==============================================================

-- tr_Familias_AuditLog


-- â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
-- 5. Familias â€” INSERT (nova famÃ­lia) + UPDATE (prefixo / tipo / ativo) + DELETE
--    Regista alteraÃ§Ãµes via FormGestaoCodigos (e qualquer outro DML directo).
--    O AuditLog.NrFunc Ã© 0 porque Familias nÃ£o estÃ¡ ligada a um colaborador.
-- â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   TRIGGER [dbo].[tr_Familias_AuditLog]
ON [dbo].[Familias]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @quem INT = COALESCE(CAST(SESSION_CONTEXT(N'NrFunc') AS INT), 0);

    -- INSERT: famÃ­lia nova
    INSERT INTO [dbo].[AuditLog] (NrFunc, AlteradoPor, Acao)
    SELECT 0, @quem,
           N'FamÃ­lia criada: ' + i.Nome
           + N' (prefixo ' + ISNULL(i.Prefixo, N'??') + N')'
    FROM inserted i
    LEFT JOIN deleted d ON i.Nome = d.Nome
    WHERE d.Nome IS NULL;

    -- UPDATE: Prefixo
    INSERT INTO [dbo].[AuditLog] (NrFunc, AlteradoPor, Acao)
    SELECT 0, @quem,
           N'FamÃ­lia ' + i.Nome + N': prefixo '
           + ISNULL(d.Prefixo, N'??') + N' â†’ ' + ISNULL(i.Prefixo, N'??')
    FROM inserted i
    JOIN deleted d ON i.Nome = d.Nome
    WHERE ISNULL(i.Prefixo, N'') <> ISNULL(d.Prefixo, N'');

    -- UPDATE: NomeVista
    INSERT INTO [dbo].[AuditLog] (NrFunc, AlteradoPor, Acao)
    SELECT 0, @quem,
           N'FamÃ­lia ' + i.Nome + N': NomeVista "'
           + ISNULL(d.NomeVista, N'') + N'" â†’ "' + ISNULL(i.NomeVista, N'') + N'"'
    FROM inserted i
    JOIN deleted d ON i.Nome = d.Nome
    WHERE ISNULL(i.NomeVista, N'') <> ISNULL(d.NomeVista, N'');

    -- UPDATE: TipoTamanho
    INSERT INTO [dbo].[AuditLog] (NrFunc, AlteradoPor, Acao)
    SELECT 0, @quem,
           N'FamÃ­lia ' + i.Nome + N': tipo '
           + ISNULL(d.TipoTamanho, N'') + N' â†’ ' + ISNULL(i.TipoTamanho, N'')
    FROM inserted i
    JOIN deleted d ON i.Nome = d.Nome
    WHERE ISNULL(i.TipoTamanho, N'') <> ISNULL(d.TipoTamanho, N'');

    -- UPDATE: Ativo (ativar/desativar)
    INSERT INTO [dbo].[AuditLog] (NrFunc, AlteradoPor, Acao)
    SELECT 0, @quem,
           N'FamÃ­lia ' + i.Nome + N': '
           + CASE WHEN i.Ativo = 1 THEN N'ativada' ELSE N'desativada' END
    FROM inserted i
    JOIN deleted d ON i.Nome = d.Nome
    WHERE i.Ativo <> d.Ativo;

    -- DELETE
    INSERT INTO [dbo].[AuditLog] (NrFunc, AlteradoPor, Acao)
    SELECT 0, @quem,
           N'FamÃ­lia eliminada: ' + d.Nome
           + N' (prefixo ' + ISNULL(d.Prefixo, N'??') + N')'
    FROM deleted d
    LEFT JOIN inserted i ON d.Nome = i.Nome
    WHERE i.Nome IS NULL;
END;

GO

-- tr_Funcionarios_AuditLog


-- â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
-- 2. Funcionarios â€” UPDATE (alteraÃ§Ãµes ao perfil do colaborador)
--    Regista coluna a coluna para saber exatamente o que mudou.
-- â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   TRIGGER [dbo].[tr_Funcionarios_AuditLog]
ON [dbo].[Funcionarios]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @quem INT = COALESCE(CAST(SESSION_CONTEXT(N'NrFunc') AS INT), 0);

    -- Nome alterado
    IF UPDATE(Nome)
        INSERT INTO [dbo].[AuditLog] (NrFunc, AlteradoPor, Acao)
        SELECT i.Nr, @quem,
               N'Nome: "' + d.Nome + N'" â†’ "' + i.Nome + N'"'
        FROM inserted i JOIN deleted d ON i.Nr = d.Nr
        WHERE i.Nome <> d.Nome;

    -- FunÃ§Ã£o/perfil alterado
    IF UPDATE(FuncaoID)
        INSERT INTO [dbo].[AuditLog] (NrFunc, AlteradoPor, Acao)
        SELECT i.Nr, @quem,
               N'FuncaoID: ' + CAST(d.FuncaoID AS NVARCHAR(10))
               + N' â†’ ' + CAST(i.FuncaoID AS NVARCHAR(10))
        FROM inserted i JOIN deleted d ON i.Nr = d.Nr
        WHERE ISNULL(i.FuncaoID, -1) <> ISNULL(d.FuncaoID, -1);

    -- Estabelecimento alterado
    IF UPDATE(EstabID)
        INSERT INTO [dbo].[AuditLog] (NrFunc, AlteradoPor, Acao)
        SELECT i.Nr, @quem,
               N'EstabID: ' + ISNULL(CAST(d.EstabID AS NVARCHAR(10)), N'NULL')
               + N' â†’ ' + ISNULL(CAST(i.EstabID AS NVARCHAR(10)), N'NULL')
        FROM inserted i JOIN deleted d ON i.Nr = d.Nr
        WHERE ISNULL(i.EstabID, -1) <> ISNULL(d.EstabID, -1);
END;

GO

-- tr_FuncionarioTamanhos_AuditLog


-- â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
-- 4. FuncionarioTamanhos â€” INSERT + UPDATE (tamanhos de fardamento)
--    Exclui Nr = 0 (linha de defaults do sistema â€” nÃ£o Ã© um colaborador real).
-- â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   TRIGGER [dbo].[tr_FuncionarioTamanhos_AuditLog]
ON [dbo].[FuncionarioTamanhos]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @quem INT = COALESCE(CAST(SESSION_CONTEXT(N'NrFunc') AS INT), 0);

    -- INSERT: tamanho definido pela primeira vez
    IF NOT EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO [dbo].[AuditLog] (NrFunc, AlteradoPor, Acao)
        SELECT i.Nr, @quem,
               N'Tamanho definido: ' + i.Familia + N' = ' + i.Tamanho
        FROM inserted i
        WHERE i.Nr > 0;
        RETURN;
    END

    -- UPDATE: tamanho alterado
    INSERT INTO [dbo].[AuditLog] (NrFunc, AlteradoPor, Acao)
    SELECT i.Nr, @quem,
           N'Tamanho alterado: ' + i.Familia
           + N' ' + d.Tamanho + N' â†’ ' + i.Tamanho
    FROM inserted i
    JOIN deleted d ON i.Nr = d.Nr AND i.Familia = d.Familia
    WHERE i.Tamanho <> d.Tamanho
      AND i.Nr > 0;
END;

GO

-- tr_Login_AuditLog


-- â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
-- 3. Login â€” UPDATE (alteraÃ§Ã£o ou reposiÃ§Ã£o de password)
--    Regista sempre que a coluna Password Ã© alvo de UPDATE, mesmo que o hash
--    seja igual (ex: reposiÃ§Ã£o para o valor padrÃ£o jÃ¡ existente).
--    SESSION_CONTEXT(N'PasswordAcao') = 'reposta' â†’ "Password reposta"
--    Sem contexto â†’ "Password alterada"  (FormNovaPasse)
-- â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   TRIGGER [dbo].[tr_Login_AuditLog]
ON [dbo].[Login]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT UPDATE(Password) RETURN;

    DECLARE @quem  INT          = COALESCE(CAST(SESSION_CONTEXT(N'NrFunc')        AS INT),          0);
    DECLARE @acao  NVARCHAR(20) = COALESCE(CAST(SESSION_CONTEXT(N'PasswordAcao') AS NVARCHAR(20)), N'alterada');

    INSERT INTO [dbo].[AuditLog] (NrFunc, AlteradoPor, Acao)
    SELECT
        i.Nr,
        COALESCE(@quem, i.Nr),
        N'Password ' + @acao
    FROM inserted i
    JOIN deleted d ON i.Nr = d.Nr;
END;

GO

-- tr_PedidoRegistos_AuditLog

-- â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
-- TRIGGERS DE AUDITORIA â€” PEPIDI
-- â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
--
-- Mecanismo: SESSION_CONTEXT('NrFunc')
--   A aplicaÃ§Ã£o define o utilizador autenticado via GetConn.SetContext(conn)
--   (que chama sp_set_session_context) antes de executar operaÃ§Ãµes auditadas.
--   Os triggers leem este valor para preencher AlteradoPor no AuditLog.
--   Se o contexto nÃ£o estiver definido (agente, sistema), AlteradoPor = 0.
--
-- Fallback seguro: sem FK em AuditLog.AlteradoPor â†’ 0 Ã© sempre vÃ¡lido.
-- SET NOCOUNT ON em todos os triggers para nÃ£o interferir com @@ROWCOUNT
-- da aplicaÃ§Ã£o cliente.
-- â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•


-- â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
-- 1. PedidoRegistos â€” INSERT (novo pedido) + UPDATE (mudanÃ§a de estado)
-- â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   TRIGGER [dbo].[tr_PedidoRegistos_AuditLog]
ON [dbo].[PedidoRegistos]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @quem INT = COALESCE(CAST(SESSION_CONTEXT(N'NrFunc') AS INT), 0);

    -- INSERT: novo pedido submetido
    IF NOT EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO [dbo].[AuditLog] (NrFunc, AlteradoPor, Acao)
        SELECT
            i.NrFunc,
            COALESCE(@quem, COALESCE(i.CriadoPor, 0)),
            N'Pedido #' + CAST(i.ID AS NVARCHAR(10)) + N' criado'
        FROM inserted i;
        RETURN;
    END

    -- UPDATE: registar apenas quando o Estado muda
    INSERT INTO [dbo].[AuditLog] (NrFunc, AlteradoPor, Acao)
    SELECT
        i.NrFunc,
        COALESCE(@quem, COALESCE(i.AlteradoPor, i.AprovadoPor, 0)),
        N'Pedido #' + CAST(i.ID AS NVARCHAR(10)) + N': '
            + d.Estado + N' â†’ ' + i.Estado
    FROM inserted i
    JOIN deleted d ON i.ID = d.ID
    WHERE i.Estado <> d.Estado;
END;

GO

-- ==============================================================
-- PARTE 5: DADOS (referencia + demo)
-- ==============================================================

-- Cor
INSERT INTO [Cor] ([ID],[Nome]) VALUES (1,N'Branco          ');
INSERT INTO [Cor] ([ID],[Nome]) VALUES (2,N'Azul            ');
INSERT INTO [Cor] ([ID],[Nome]) VALUES (3,N'Laranja         ');

-- Estado
INSERT INTO [Estado] ([ID],[Descricao]) VALUES (1,N'DisponÃ­vel');
INSERT INTO [Estado] ([ID],[Descricao]) VALUES (2,N'Esgotado');

-- Estabelecimentos
INSERT INTO [Estabelecimentos] ([ID],[Nome]) VALUES (1,N'E0100 (Central de Distribuição)');
INSERT INTO [Estabelecimentos] ([ID],[Nome]) VALUES (2,N'E0101 (Costa Do Valado)');
INSERT INTO [Estabelecimentos] ([ID],[Nome]) VALUES (3,N'E0102 (Z. I. Palhaça)');

-- Acessos
SET IDENTITY_INSERT [Acessos] ON;
INSERT INTO [Acessos] ([ID],[Descricao]) VALUES (1,N'Geral');
INSERT INTO [Acessos] ([ID],[Descricao]) VALUES (2,NULL);
INSERT INTO [Acessos] ([ID],[Descricao]) VALUES (3,NULL);
INSERT INTO [Acessos] ([ID],[Descricao]) VALUES (4,NULL);
SET IDENTITY_INSERT [Acessos] OFF;

-- Funcoes
SET IDENTITY_INSERT [Funcoes] ON;
INSERT INTO [Funcoes] ([ID],[Nome],[NivelAcesso],[PodeVerStock],[PodeInserirStock],[PodeCriarStock],[PodeVerHistorico],[PodeEditarFunc],[PodeSubmeter],[PodeAprovar],[PodeEntregar],[PodeCriarFuncoes],[PodeAlterarDefinicoes],[PodeVerUsados],[CorHex],[CriadoEm],[CriadoPor],[AlteradoEm],[AlteradoPor]) VALUES (1,N'Admin',0,NULL,NULL,NULL,NULL,NULL,0,NULL,NULL,NULL,NULL,NULL,N'#2E86DE   ',N'2026-06-19 00:00:00',1077,N'2026-06-16 00:00:00',1077);
INSERT INTO [Funcoes] ([ID],[Nome],[NivelAcesso],[PodeVerStock],[PodeInserirStock],[PodeCriarStock],[PodeVerHistorico],[PodeEditarFunc],[PodeSubmeter],[PodeAprovar],[PodeEntregar],[PodeCriarFuncoes],[PodeAlterarDefinicoes],[PodeVerUsados],[CorHex],[CriadoEm],[CriadoPor],[AlteradoEm],[AlteradoPor]) VALUES (2,N'Produção',1,0,NULL,NULL,0,0,NULL,0,0,0,0,NULL,N'#E67E22   ',N'2026-06-19 08:40:57',1077,NULL,NULL);
INSERT INTO [Funcoes] ([ID],[Nome],[NivelAcesso],[PodeVerStock],[PodeInserirStock],[PodeCriarStock],[PodeVerHistorico],[PodeEditarFunc],[PodeSubmeter],[PodeAprovar],[PodeEntregar],[PodeCriarFuncoes],[PodeAlterarDefinicoes],[PodeVerUsados],[CorHex],[CriadoEm],[CriadoPor],[AlteradoEm],[AlteradoPor]) VALUES (3,N'RH',0,NULL,NULL,NULL,NULL,NULL,0,NULL,NULL,NULL,NULL,NULL,N'#8000FF   ',N'2026-07-20 10:44:52',1077,N'2026-07-20 11:57:32',1077);
INSERT INTO [Funcoes] ([ID],[Nome],[NivelAcesso],[PodeVerStock],[PodeInserirStock],[PodeCriarStock],[PodeVerHistorico],[PodeEditarFunc],[PodeSubmeter],[PodeAprovar],[PodeEntregar],[PodeCriarFuncoes],[PodeAlterarDefinicoes],[PodeVerUsados],[CorHex],[CriadoEm],[CriadoPor],[AlteradoEm],[AlteradoPor]) VALUES (7,N'Encarregado',2,0,0,0,NULL,0,NULL,0,0,0,0,0,N'#C9A96E   ',N'2026-07-20 11:22:08',1077,N'2026-07-20 11:59:22',2001);
INSERT INTO [Funcoes] ([ID],[Nome],[NivelAcesso],[PodeVerStock],[PodeInserirStock],[PodeCriarStock],[PodeVerHistorico],[PodeEditarFunc],[PodeSubmeter],[PodeAprovar],[PodeEntregar],[PodeCriarFuncoes],[PodeAlterarDefinicoes],[PodeVerUsados],[CorHex],[CriadoEm],[CriadoPor],[AlteradoEm],[AlteradoPor]) VALUES (8,N'Manutenção',1,0,0,0,NULL,0,NULL,0,0,0,0,0,N'#1A3E6E   ',N'2026-07-20 11:22:08',1077,NULL,NULL);
INSERT INTO [Funcoes] ([ID],[Nome],[NivelAcesso],[PodeVerStock],[PodeInserirStock],[PodeCriarStock],[PodeVerHistorico],[PodeEditarFunc],[PodeSubmeter],[PodeAprovar],[PodeEntregar],[PodeCriarFuncoes],[PodeAlterarDefinicoes],[PodeVerUsados],[CorHex],[CriadoEm],[CriadoPor],[AlteradoEm],[AlteradoPor]) VALUES (9,N'Logística',1,0,0,0,NULL,0,NULL,0,0,0,0,0,N'#7EC8E3   ',N'2026-07-20 11:22:08',1077,NULL,NULL);
INSERT INTO [Funcoes] ([ID],[Nome],[NivelAcesso],[PodeVerStock],[PodeInserirStock],[PodeCriarStock],[PodeVerHistorico],[PodeEditarFunc],[PodeSubmeter],[PodeAprovar],[PodeEntregar],[PodeCriarFuncoes],[PodeAlterarDefinicoes],[PodeVerUsados],[CorHex],[CriadoEm],[CriadoPor],[AlteradoEm],[AlteradoPor]) VALUES (10,N'Designer',NULL,NULL,0,0,NULL,NULL,NULL,0,0,0,0,NULL,N'#FF00FF   ',N'2026-08-05 10:36:37',1077,N'2026-08-05 11:37:34',1077);
INSERT INTO [Funcoes] ([ID],[Nome],[NivelAcesso],[PodeVerStock],[PodeInserirStock],[PodeCriarStock],[PodeVerHistorico],[PodeEditarFunc],[PodeSubmeter],[PodeAprovar],[PodeEntregar],[PodeCriarFuncoes],[PodeAlterarDefinicoes],[PodeVerUsados],[CorHex],[CriadoEm],[CriadoPor],[AlteradoEm],[AlteradoPor]) VALUES (11,N'SST',NULL,NULL,NULL,NULL,NULL,NULL,0,0,0,0,0,NULL,N'#FF0000   ',N'2026-08-05 10:36:58',1077,N'2026-08-05 11:37:44',1077);
SET IDENTITY_INSERT [Funcoes] OFF;

-- AcessoFuncoes
SET IDENTITY_INSERT [AcessoFuncoes] ON;
INSERT INTO [AcessoFuncoes] ([ID],[AcessoID],[FuncaoID]) VALUES (1,2,2);
INSERT INTO [AcessoFuncoes] ([ID],[AcessoID],[FuncaoID]) VALUES (2,2,1);
INSERT INTO [AcessoFuncoes] ([ID],[AcessoID],[FuncaoID]) VALUES (3,3,2);
INSERT INTO [AcessoFuncoes] ([ID],[AcessoID],[FuncaoID]) VALUES (4,4,7);
INSERT INTO [AcessoFuncoes] ([ID],[AcessoID],[FuncaoID]) VALUES (5,4,9);
INSERT INTO [AcessoFuncoes] ([ID],[AcessoID],[FuncaoID]) VALUES (6,4,8);
INSERT INTO [AcessoFuncoes] ([ID],[AcessoID],[FuncaoID]) VALUES (7,4,2);
SET IDENTITY_INSERT [AcessoFuncoes] OFF;

-- Familias
INSERT INTO [Familias] ([Nome],[NomeVista],[TipoTamanho],[Ativo],[Prefixo]) VALUES (N'Bata',N'Bata de Trabalho',N'Letra',NULL,N'10');
INSERT INTO [Familias] ([Nome],[NomeVista],[TipoTamanho],[Ativo],[Prefixo]) VALUES (N'Calca',N'Calça de Trabalho',N'Numero',NULL,N'02');
INSERT INTO [Familias] ([Nome],[NomeVista],[TipoTamanho],[Ativo],[Prefixo]) VALUES (N'Casaco',N'Casaco Câmara Fria',N'Letra',NULL,N'07');
INSERT INTO [Familias] ([Nome],[NomeVista],[TipoTamanho],[Ativo],[Prefixo]) VALUES (N'PoloMCompr',N'Polo Manga Comprida',N'Letra',NULL,N'03');
INSERT INTO [Familias] ([Nome],[NomeVista],[TipoTamanho],[Ativo],[Prefixo]) VALUES (N'TShirt',N'T-Shirt',N'Letra',NULL,N'01');

-- Funcionarios
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (1,N'Vitor Baia',1,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (2,N'João Pinto',10,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (3,N'Pepe',10,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (4,N'Jorge Costa',7,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (5,N'Fernando Couto',11,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (6,N'Costinha',9,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (7,N'Ricardo Quaresma',2,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (8,N'Lucho González',8,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (9,N'Radamel Falcao',9,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (10,N'Deco Souza',2,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (11,N'Derlei Oliveira',7,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (12,N'Hulk Givanildo',11,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (13,N'Alex Telles',8,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (14,N'Rolando Fonseca',11,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (15,N'Héctor Herrera',2,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (16,N'Jesús Corona',10,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (17,N'Maniche Ribeiro',7,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (18,N'Carlos Alberto',2,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (19,N'Cristian Săpunaru',2,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (20,N'Otávio Monteiro',8,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (21,N'Sérgio Oliveira',2,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (22,N'Hélder Postiga',2,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (23,N'Benni McCarthy',11,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (24,N'Diogo Costa',11,2);
INSERT INTO [Funcionarios] ([Nr],[Nome],[FuncaoID],[EstabID]) VALUES (1077,N'Bruno Oliveira',1,2);

-- Login
INSERT INTO [Login] ([Nr],[Password]) VALUES (1,N'6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b');
INSERT INTO [Login] ([Nr],[Password]) VALUES (2,N'd4735e3a265e16eee03f59718b9b5d03019c07d8b6c51f90da3a666eec13ab35');
INSERT INTO [Login] ([Nr],[Password]) VALUES (3,N'4e07408562bedb8b60ce05c1decfe3ad16b72230967de01f640b7e4729b49fce');
INSERT INTO [Login] ([Nr],[Password]) VALUES (4,N'4b227777d4dd1fc61c6f884f48641d02b4d121d3fd328cb08b5531fcacdabf8a');
INSERT INTO [Login] ([Nr],[Password]) VALUES (5,N'ef2d127de37b942baad06145e54b0c619a1f22327b2ebbcfbec78f5564afe39d');
INSERT INTO [Login] ([Nr],[Password]) VALUES (6,N'e7f6c011776e8db7cd330b54174fd76f7d0216b612387a5ffcfb81e6f0919683');
INSERT INTO [Login] ([Nr],[Password]) VALUES (7,N'7902699be42c8a8e46fbbb4501726517e86b22c56a189f7625a6da49081b2451');
INSERT INTO [Login] ([Nr],[Password]) VALUES (8,N'2c624232cdd221771294dfbb310aca000a0df6ac8b66b696d90ef06fdefb64a3');
INSERT INTO [Login] ([Nr],[Password]) VALUES (9,N'19581e27de7ced00ff1ce50b2047e7a567c76b1cbaebabe5ef03f7c3017bb5b7');
INSERT INTO [Login] ([Nr],[Password]) VALUES (10,N'4a44dc15364204a80fe80e9039455cc1608281820fe2b24f1e5233ade6af1dd5');
INSERT INTO [Login] ([Nr],[Password]) VALUES (11,N'4fc82b26aecb47d2868c4efbe3581732a3e7cbcc6c2efb32062c08170a05eeb8');
INSERT INTO [Login] ([Nr],[Password]) VALUES (12,N'6b51d431df5d7f141cbececcf79edf3dd861c3b4069f0b11661a3eefacbba918');
INSERT INTO [Login] ([Nr],[Password]) VALUES (13,N'3fdba35f04dc8c462986c992bcf875546257113072a909c162f7e470e581e278');
INSERT INTO [Login] ([Nr],[Password]) VALUES (14,N'8527a891e224136950ff32ca212b45bc93f69fbb801c3b1ebedac52775f99e61');
INSERT INTO [Login] ([Nr],[Password]) VALUES (15,N'e629fa6598d732768f7c726b4b621285f9c3b85303900aa912017db7617d8bdb');
INSERT INTO [Login] ([Nr],[Password]) VALUES (16,N'b17ef6d19c7a5b1ee83b907c595526dcb1eb06db8227d650d5dda0a9f4ce8cd9');
INSERT INTO [Login] ([Nr],[Password]) VALUES (17,N'4523540f1504cd17100c4835e85b7eefd49911580f8efff0599a8f283be6b9e3');
INSERT INTO [Login] ([Nr],[Password]) VALUES (18,N'4ec9599fc203d176a301536c2e091a19bc852759b255bd6818810a42c5fed14a');
INSERT INTO [Login] ([Nr],[Password]) VALUES (19,N'9400f1b21cb527d7fa3d3eabba93557a18ebe7a2ca4e471cfe5e4c5b4ca7f767');
INSERT INTO [Login] ([Nr],[Password]) VALUES (20,N'f5ca38f748a1d6eaf726b8a42fb575c3c71f1864a8143301782de13da2d9202b');
INSERT INTO [Login] ([Nr],[Password]) VALUES (21,N'6f4b6612125fb3a0daecd2799dfd6c9c299424fd920f9b308110a2c1fbd8f443');
INSERT INTO [Login] ([Nr],[Password]) VALUES (22,N'785f3ec7eb32f30b90cd0fcf3657d388b5ff4297f2f9716ff66e9b69c05ddd09');
INSERT INTO [Login] ([Nr],[Password]) VALUES (23,N'535fa30d7e25dd8a49f1536779734ec8286108d115da5045d77f3b4185d8f790');
INSERT INTO [Login] ([Nr],[Password]) VALUES (24,N'c2356069e9d1e79ca924378153cfbbfb4d4416b1f99d41a2940bfdb66c5319db');
INSERT INTO [Login] ([Nr],[Password]) VALUES (1077,N'ca978112ca1bbdcafac231b39a23dc4da786eff8147c4e72b9807785afee48bb');

-- FuncionarioTamanhos
INSERT INTO [FuncionarioTamanhos] ([Nr],[Familia],[Tamanho]) VALUES (6,N'Tshirt',N'L');
INSERT INTO [FuncionarioTamanhos] ([Nr],[Familia],[Tamanho]) VALUES (7,N'Bata',N'L');
INSERT INTO [FuncionarioTamanhos] ([Nr],[Familia],[Tamanho]) VALUES (7,N'Tshirt',N'L');

-- EPI
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (10010104,N'Bata',N'Bata manga comprida',N'M',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (10010105,N'Bata',N'Bata manga comprida',N'L',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (10020103,N'Bata',N'Bata manga curta',N'S',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (10030103,N'Bata',N'Batas manga comprida',N'S',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (10030105,N'Bata',N'Batas manga comprida',N'L',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (10040104,N'Bata',N'Bata manga curta - NOVO',N'M',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (10040105,N'Bata',N'Bata manga curta - NOVO',N'L',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (10050102,N'Bata',N'Bata senhora comprida',N'XS',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (10050103,N'Bata',N'Bata senhora comprida',N'S',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (10050104,N'Bata',N'Bata senhora comprida',N'M',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (10060104,N'Bata',N'Bata Homem comprida',N'M',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (10060105,N'Bata',N'Bata Homem comprida',N'L',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (70010103,N'Casaco',N'Casaco manga comprida - cardado',N'S',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (70010105,N'Casaco',N'Casaco manga comprida - cardado',N'L',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (80010111,N'Calca',N'Calças',N'38',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (80010113,N'Calca',N'Calças',N'40',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (80010115,N'Calca',N'Calças',N'42',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (80010117,N'Calca',N'Calças',N'44',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (80010119,N'Calca',N'Calças',N'46',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (99010100,N'Boné',N'Boné',NULL,1,3,0.00,0);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (99010103,N'Tshirt',N'T-shirt',N'S',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (99010104,N'Tshirt',N'T-shirt',N'M',1,4,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (99010105,N'Tshirt',N'T-shirt',N'L',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (99010106,N'PoloMcomp',N'Polo felpa manga comprida - cardado',N'XL',1,3,0.00,0);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (99020100,N'Boné',N'Bonés',NULL,1,3,0.00,0);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (99020103,N'TShirt',N'T-shirt manga comprida',N'S',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (99020106,N'PoloMComp',N'Pólo manga comprida - cardado',N'XL',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (99030103,N'TShirt',N'Blusões polares "CHAQUETON POLAR REF. ª 04035"',N'S',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (99030104,N'TShirt',N'Blusões polares "CHAQUETON POLAR REF. ª 04035"',N'M',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (99030106,N'TShirt',N'Blusões polares "CHAQUETON POLAR REF. ª 04035"',N'XL',1,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (99040304,N'TShirt',N'BLUSÃO IMPERMEÁVEL 3 EM 1 AV. PW REF. C465 AZUL/LARANJA',N'M',3,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (99040305,N'TShirt',N'BLUSÃO IMPERMEÁVEL 3 EM 1 AV. PW REF. C465 AZUL/LARANJA',N'L',3,3,0.00,NULL);
INSERT INTO [EPI] ([Codigo],[Familia],[Modelo],[Tamanho],[CorID],[AcessoID],[Preco],[Ativo]) VALUES (99040306,N'TShirt',N'BLUSÃO IMPERMEÁVEL 3 EM 1 AV. PW REF. C465 AZUL/LARANJA',N'XL',3,3,0.00,NULL);

-- Stock
SET IDENTITY_INSERT [Stock] ON;
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (1,99010105,1,99);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (2,99010104,1,71);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (3,10010104,1,3);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (4,10010105,1,4);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (5,80010113,1,31);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (6,10020103,1,6);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (7,99010106,1,76);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (8,10030103,1,11);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (9,10030105,1,12);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (10,99010100,1,13);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (11,80010111,1,41);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (12,80010119,1,15);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (13,80010117,1,45);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (14,99010103,1,59);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (15,99020100,1,23);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (16,10040104,1,24);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (17,10040105,1,25);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (18,80010115,1,28);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (19,10050102,1,30);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (20,10050103,1,31);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (21,10050104,1,31);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (22,10060104,1,33);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (23,10060105,1,33);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (24,99020103,1,35);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (25,99020106,1,36);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (26,70010105,1,39);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (27,70010103,1,40);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (28,99030103,1,42);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (29,99030104,1,44);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (30,99030106,1,45);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (31,99040304,1,46);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (32,99040305,1,47);
INSERT INTO [Stock] ([ID],[Codigo],[Estado],[Quantidade]) VALUES (33,99040306,1,48);
SET IDENTITY_INSERT [Stock] OFF;

-- Dispositivos
SET IDENTITY_INSERT [Dispositivos] ON;
SET IDENTITY_INSERT [Dispositivos] OFF;

-- PedidoRegistos
SET IDENTITY_INSERT [PedidoRegistos] ON;
INSERT INTO [PedidoRegistos] ([ID],[Data],[NrFunc],[Estado],[AprovadoPor],[EntregadoPor],[CaminhoPDF],[Notas],[CriacaoData],[CriadoPor],[AlteracaoData],[AlteradoPor]) VALUES (1,N'2026-08-05 00:00:00',7,N'Finalizado',1077,1077,N'C:\Users\bruno.oliveira\Desktop\ComprovativoNr00001.pdf',N'
[Finalizado por Bruno Oliveira com assinatura de Ricardo Quaresma]',N'2026-08-05 11:49:50',NULL,N'2026-08-05 11:50:30',1077);
INSERT INTO [PedidoRegistos] ([ID],[Data],[NrFunc],[Estado],[AprovadoPor],[EntregadoPor],[CaminhoPDF],[Notas],[CriacaoData],[CriadoPor],[AlteracaoData],[AlteradoPor]) VALUES (2,N'2026-08-06 00:00:00',6,N'Finalizado',1077,1077,N'U:\Bruno\ComprovativosPEPIDI\ComprovativoNr00002.pdf',N'TESTE
[Bruno Oliveira]: Alterou a quantidade de ''T-shirt Branco (L)'' de 2 para 5.
[Finalizado por Bruno Oliveira com assinatura de Costinha]',N'2026-08-06 12:33:09',NULL,N'2026-08-06 12:34:07',1077);
INSERT INTO [PedidoRegistos] ([ID],[Data],[NrFunc],[Estado],[AprovadoPor],[EntregadoPor],[CaminhoPDF],[Notas],[CriacaoData],[CriadoPor],[AlteracaoData],[AlteradoPor]) VALUES (3,N'2026-08-06 00:00:00',10,N'Finalizado',1077,1077,N'U:\Bruno\ComprovativosPEPIDI\ComprovativoNr00003.pdf',N'
[Finalizado por Bruno Oliveira com assinatura de Deco Souza]',N'2026-08-06 18:16:55',NULL,N'2026-08-06 18:17:37',1077);
SET IDENTITY_INSERT [PedidoRegistos] OFF;

-- PedidoPacote
SET IDENTITY_INSERT [PedidoPacote] ON;
INSERT INTO [PedidoPacote] ([ID],[IDPedidoRegisto],[CodigoEPI],[Quantidade],[IDStock],[TipoMovimento]) VALUES (1,1,10060105,1,23,N'P');
INSERT INTO [PedidoPacote] ([ID],[IDPedidoRegisto],[CodigoEPI],[Quantidade],[IDStock],[TipoMovimento]) VALUES (2,1,99010105,1,1,N'P');
INSERT INTO [PedidoPacote] ([ID],[IDPedidoRegisto],[CodigoEPI],[Quantidade],[IDStock],[TipoMovimento]) VALUES (3,1,99020106,1,25,N'P');
INSERT INTO [PedidoPacote] ([ID],[IDPedidoRegisto],[CodigoEPI],[Quantidade],[IDStock],[TipoMovimento]) VALUES (4,2,99010105,5,1,N'P');
INSERT INTO [PedidoPacote] ([ID],[IDPedidoRegisto],[CodigoEPI],[Quantidade],[IDStock],[TipoMovimento]) VALUES (5,3,10050104,1,21,N'P');
INSERT INTO [PedidoPacote] ([ID],[IDPedidoRegisto],[CodigoEPI],[Quantidade],[IDStock],[TipoMovimento]) VALUES (6,3,99030103,1,28,N'P');
SET IDENTITY_INSERT [PedidoPacote] OFF;

-- AuditLog
SET IDENTITY_INSERT [AuditLog] ON;
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (1,0,N'2026-08-05 10:45:41',1077,N'FamÃ­lia eliminada: Touca (prefixo 20)');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (2,0,N'2026-08-05 10:45:41',1077,N'FamÃ­lia eliminada: Luvas (prefixo 30)');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (3,0,N'2026-08-05 10:45:41',1077,N'FamÃ­lia eliminada: Mascara (prefixo 40)');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (4,0,N'2026-08-05 10:45:41',1077,N'FamÃ­lia eliminada: Avental (prefixo 50)');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (5,0,N'2026-08-05 10:45:41',1077,N'FamÃ­lia eliminada: Botas (prefixo 60)');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (6,0,N'2026-08-05 10:45:41',1077,N'FamÃ­lia criada: TShirt (prefixo 01)');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (7,0,N'2026-08-05 10:45:41',1077,N'FamÃ­lia Calca: prefixo 80 â†’ 02');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (8,0,N'2026-08-05 10:45:41',1077,N'FamÃ­lia Casaco: prefixo 70 â†’ 07');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (9,0,N'2026-08-05 10:46:47',1077,N'FamÃ­lia criada: PoloMCompr (prefixo 03)');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (10,7,N'2026-08-05 10:49:24',0,N'Tamanho definido: Bata = L');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (11,7,N'2026-08-05 10:49:43',0,N'Tamanho definido: Tshirt = L');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (12,7,N'2026-08-05 10:49:50',0,N'Pedido #1 criado');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (13,7,N'2026-08-05 10:50:13',1077,N'Pedido #1: Pendente â†’ Aprovado');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (14,7,N'2026-08-05 10:50:30',1077,N'Pedido #1: Aprovado â†’ Finalizado');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (15,6,N'2026-08-06 11:33:07',0,N'Tamanho definido: Tshirt = L');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (16,6,N'2026-08-06 11:33:09',0,N'Pedido #2 criado');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (17,6,N'2026-08-06 11:33:44',1077,N'Pedido #2: Pendente â†’ Aprovado');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (18,6,N'2026-08-06 11:34:07',1077,N'Pedido #2: Aprovado â†’ Finalizado');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (19,10,N'2026-08-06 17:16:55',0,N'Pedido #3 criado');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (20,10,N'2026-08-06 17:17:08',1077,N'Pedido #3: Pendente â†’ Aprovado');
INSERT INTO [AuditLog] ([ID],[NrFunc],[DataAlteracao],[AlteradoPor],[Acao]) VALUES (21,10,N'2026-08-06 17:17:37',1077,N'Pedido #3: Aprovado â†’ Finalizado');
SET IDENTITY_INSERT [AuditLog] OFF;

-- Definicoes
SET IDENTITY_INSERT [Definicoes] ON;
INSERT INTO [Definicoes] ([ID],[Chave],[Valor],[Tipo],[DataAlteracao],[AlteradoPor]) VALUES (1,N'CaminhoComprovativos',N'U:\Bruno\ComprovativosPEPIDI',N'String',N'2026-08-06 12:30:06',1077);
INSERT INTO [Definicoes] ([ID],[Chave],[Valor],[Tipo],[DataAlteracao],[AlteradoPor]) VALUES (2,N'CaminhoRelatorios',N'U:\Bruno\ComprovativosPEPIDI\Relatorios',N'String',N'2026-08-06 12:30:11',1077);
INSERT INTO [Definicoes] ([ID],[Chave],[Valor],[Tipo],[DataAlteracao],[AlteradoPor]) VALUES (3,N'StockMinimo',N'10',N'Int',N'2026-06-23 15:22:42',1077);
SET IDENTITY_INSERT [Definicoes] OFF;

-- QueriesSalvas
SET IDENTITY_INSERT [QueriesSalvas] ON;
INSERT INTO [QueriesSalvas] ([ID],[Nome],[ConteudoSQL],[ConfigFiltros]) VALUES (1,N'Geral',N'SELECT 
                            E.Codigo, 
                            E.Modelo, 
                            E.Tamanho, 
                            ISNULL(STRING_AGG(F.Nome, '' | ''), ''Sem Função'') AS NomeFuncao, 
                            ISNULL(STRING_AGG(F.CorHex, '',''), ''#808080'') AS CorFuncao, 
                            S.Quantidade 
                        FROM EPI E 
                        LEFT JOIN AcessoFuncoes AF ON E.AcessoID = AF.AcessoID 
                        LEFT JOIN Stock S ON E.Codigo = S.Codigo 
                        LEFT JOIN Funcoes F ON AF.FuncaoID = F.ID WHERE S.Estado = 1  GROUP BY E.Modelo, E.Tamanho, S.Quantidade, E.AcessoID, E.Codigo',N'Funcoes:|Familia:|Modelo:|Tamanho:');
SET IDENTITY_INSERT [QueriesSalvas] OFF;

-- RegrasFamilia
SET IDENTITY_INSERT [RegrasFamilia] ON;
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (1,N'bata',N'Bata');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (2,N'touca',N'Touca');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (3,N'luvas',N'Luvas');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (4,N'máscara',N'Mascara');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (5,N'mascara',N'Mascara');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (6,N'respirador',N'Mascara');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (7,N'avental',N'Avental');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (8,N'botas',N'Botas');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (9,N'sapatos segurança',N'Botas');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (10,N'casaco',N'Casaco');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (11,N'calça',N'Calca');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (12,N'calca',N'Calca');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (13,N'calças',N'Calca');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (14,N'colete',N'Colete');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (15,N'fato',N'Fato');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (16,N't-shirt',N'Tshirt');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (17,N'polo',N'PoloMCurt');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (18,N'batas',N'Bata');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (19,N'boné',N'Boné');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (20,N'bonés',N'Boné');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (21,N'pólo',N'PoloMComp');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (22,N'pólos',N'PoloMCurt');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (23,N'blusões',N'TShirt');
INSERT INTO [RegrasFamilia] ([ID],[PalavraChave],[FamiliaDestino]) VALUES (24,N'blusão',N'TShirt');
SET IDENTITY_INSERT [RegrasFamilia] OFF;

-- RegrasFuncao
SET IDENTITY_INSERT [RegrasFuncao] ON;
INSERT INTO [RegrasFuncao] ([ID],[PalavraChave],[FuncaoDestino]) VALUES (0,N'admin',N'Admin');
INSERT INTO [RegrasFuncao] ([ID],[PalavraChave],[FuncaoDestino]) VALUES (1,N'designer',N'Designer');
INSERT INTO [RegrasFuncao] ([ID],[PalavraChave],[FuncaoDestino]) VALUES (2,N'encarregado',N'Encarregado');
INSERT INTO [RegrasFuncao] ([ID],[PalavraChave],[FuncaoDestino]) VALUES (3,N'sst',N'SST');
INSERT INTO [RegrasFuncao] ([ID],[PalavraChave],[FuncaoDestino]) VALUES (4,N'logística',N'Logística');
INSERT INTO [RegrasFuncao] ([ID],[PalavraChave],[FuncaoDestino]) VALUES (5,N'produção',N'Produção');
INSERT INTO [RegrasFuncao] ([ID],[PalavraChave],[FuncaoDestino]) VALUES (6,N'manutenção',N'Manutenção');
SET IDENTITY_INSERT [RegrasFuncao] OFF;

PRINT 'PEPIDI instalado com sucesso.'
GO
