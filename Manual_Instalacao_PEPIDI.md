# PEPIDI 0.5 — Manual de Instalação

> Sistema de Gestão de EPIs · Diatosta – Indústria Alimentar, S.A.

---

## Índice

1. [Pré-requisitos](#1-pré-requisitos)
2. [Instalar a base de dados](#2-instalar-a-base-de-dados)
3. [Instalar a aplicação](#3-instalar-a-aplicação)
4. [Primeiro arranque e configuração](#4-primeiro-arranque-e-configuração)
5. [Primeiro login](#5-primeiro-login)
6. [Importar funcionários](#6-importar-funcionários)
7. [Importar EPI e Stock](#7-importar-epi-e-stock)
8. [Agente de Notificações](#8-agente-de-notificações)
9. [Reset para dados demo](#9-reset-para-dados-demo)
10. [Referência rápida — dados demo incluídos](#10-referência-rápida--dados-demo-incluídos)
11. [Notas técnicas](#11-notas-técnicas)

---

## 1. Pré-requisitos

| Componente | Versão | Obrigatório |
|---|---|---|
| Windows | 10 (1809+) / 11 | ✅ |
| .NET 9 Runtime (Desktop) | 9.x | ✅ — [download](https://dotnet.microsoft.com/download/dotnet/9.0) |
| SQL Server | 2019+ ou Express | ✅ — [download Express](https://www.microsoft.com/sql-server/sql-server-downloads) |
| SQL Server Management Studio | Qualquer | Recomendado (para configurar a BD) |
| .NET Framework 4.8 | 4.8 | Opcional (Agente de Notificações) |

> **Nota:** O instalador verifica automaticamente se o .NET 9 está presente e abre a página de download caso não esteja.

---

## 2. Instalar a base de dados

O ficheiro `PEPIDI_Setup_Completo.sql` (incluído na pasta `Setup\` da instalação) cria toda a estrutura da base de dados.

### 2.1 Passos no SSMS

1. Abrir o **SQL Server Management Studio** e ligar ao servidor SQL
2. Menu `File → Open → File...` → selecionar `PEPIDI_Setup_Completo.sql`
3. Confirmar servidor no canto inferior esquerdo
4. Clicar **Execute** (`F5`)

O script:
- Cria a base de dados `PEPIDI` (se não existir)
- Cria as 21 tabelas na ordem correta (respeita foreign keys)
- Instala 34 stored procedures e 5 triggers de auditoria
- Insere os dados de referência e os dados demo (funcionários fictícios)

### 2.2 Verificação rápida

```sql
USE PEPIDI;
SELECT name AS Tabela, (SELECT COUNT(*) FROM sys.columns WHERE object_id = t.object_id) AS Colunas
FROM sys.tables t WHERE name NOT LIKE 'sys%' ORDER BY name;
```

Devem aparecer 21 tabelas.

---

## 3. Instalar a aplicação

Execute o ficheiro **`PEPIDI_0.5_Setup.exe`** e siga o assistente:

| Passo | Descrição |
|---|---|
| 1 | Aceitar os termos |
| 2 | Escolher pasta de instalação (por defeito: `%LOCALAPPDATA%\PEPIDI`) |
| 3 | Selecionar componentes (marcar "Agente de Notificações" se pretendido) |
| 4 | Criar ícone no Ambiente de Trabalho (opcional) |
| 5 | Instalar |

Após a instalação, o PEPIDI fica acessível em:
- Menu Iniciar → **PEPIDI**
- Ambiente de Trabalho (se selecionado no passo 4)

Os ficheiros `PEPIDI_Setup_Completo.sql` e `Manual_Instalacao_PEPIDI.md` ficam instalados em `<pasta_instalação>\Setup\`.

---

## 4. Primeiro arranque e configuração

Na primeira execução, o PEPIDI não encontra o ficheiro de configuração da base de dados e abre automaticamente o ecrã de configuração:

| Campo | Exemplo |
|---|---|
| Servidor | `NOME-PC\SQLEXPRESS` |
| Base de dados | `PEPIDI` |
| Autenticação | Windows (Integrated Security) |

1. Preencher os campos
2. Clicar **Testar Ligação** — deve mostrar "Ligação bem-sucedida"
3. Clicar **Guardar**

As credenciais são encriptadas (AES-256) e guardadas em:
```
%APPDATA%\PEPIDI\conn.bin
```

Para reconfigurar noutro servidor: apagar `conn.bin` e reiniciar a aplicação.

---

## 5. Primeiro login

| Campo | Valor |
|---|---|
| Nº mecanográfico | `1077` |
| Password | `teste` |

Após login, o sistema abre o **FormGestão** (painel de administração completo).

> 💡 Para alterar password: no FormGestão → ícone do utilizador → **Alterar Password**

> ⚠️ Em produção, alterar a password do administrador antes de distribuir a outros utilizadores.

---

## 6. Importar funcionários

O PEPIDI importa funcionários a partir de um ficheiro **Excel (`.xlsx`)**.

### Formato do ficheiro

| Col. A | Col. B | Col. C | Col. D |
|---|---|---|---|
| Nº Mecanográfico | Nome Completo | Função | Estabelecimento |
| `2001` | `João Silva` | `Produção` | `E0100 (Central de Distribuição)` |
| `2002` | `Maria Santos` | `Manutenção` | `E0101 (Costa Do Valado)` |

**Regras:**
- A linha 1 é ignorada (cabeçalho)
- **Função** deve corresponder exatamente a um nome em `Funcoes.Nome`
- **Estabelecimento** deve corresponder a um valor em `Estabelecimentos.Nome`
- A password inicial de cada funcionário é automaticamente o seu nº mecanográfico

### Executar a importação

`FormGestão → Funcionários → botão Importar (ícone Excel) → selecionar ficheiro`

O sistema cria automaticamente registos em `Funcionarios`, `Login` e `FuncionarioTamanhos`.

---

## 7. Importar EPI e Stock

### Formato do ficheiro

| Col. A | Col. B | Col. C | Col. D | Col. E | Col. F |
|---|---|---|---|---|---|
| Código (8 dígitos) | Descrição | Família | Cor | Tamanho | Quantidade |
| `10010101` | `Bata Branca S` | `Bata` | `Branco` | `S` | `50` |

**Código de 8 dígitos:** `[FF][MM][CC][TT]`
- `FF` = prefixo da família (ex: `10` = Bata)
- `MM` = modelo (01, 02, ...)
- `CC` = código de cor
- `TT` = tamanho

### Executar a importação

`FormGestão → Stock → botão Importar (ícone Excel) → selecionar ficheiro`

---

## 8. Agente de Notificações

O `AgentePEPIDI.exe` (incluído se selecionado no instalador) é um processo de background que:
- Monitoriza stock abaixo do mínimo configurado
- Envia notificações sobre pedidos pendentes
- Corre na tray do Windows, com verificação a cada 5 minutos

Para configurar as notificações:
`FormGestão → Definições → Dispositivos de Notificação`

O agente lê o mesmo `conn.bin` da pasta `%APPDATA%\PEPIDI\`.

---

## 9. Reset para dados demo

Para repor o estado inicial da base de dados (apagar dados operacionais, manter dados de referência e o utilizador 1077):

```sql
USE PEPIDI;
EXEC sp_ResetPEPIDI;
```

O que o `sp_ResetPEPIDI` apaga:
- Todos os pedidos e devoluções (`PedidoPacote`, `PedidoRegistos`)
- Todos os funcionários **exceto** nº 1077
- Todo o EPI e Stock
- Registo de auditoria (`AuditLog`)

O que mantém:
- Funções, Famílias, Estabelecimentos, Cores, Acessos
- Regras de IA (`RegrasFamilia`, `RegrasFuncao`)
- Definições da aplicação
- Funcionário 1077 e respetivo login

---

## 10. Referência rápida — dados demo incluídos

### Funções

| Nome | Nível | Submete pedidos |
|---|---|---|
| Admin | 0 (total) | Não |
| RH | 0 (total) | Não |
| Encarregado | 2 | Não |
| Produção | 1 | Sim |
| Manutenção | 1 | Sim |
| Logística | 1 | Sim |

### Estabelecimentos

| Nº | Nome |
|---|---|
| 1 | E0100 (Central de Distribuição) |
| 2 | E0101 (Costa Do Valado) |
| 3 | E0102 (Z. I. Palhaça) |

### Famílias de EPI e prefixos

| Família | Prefixo | Tipo tamanho |
|---|---|---|
| Bata | 10 | Letra (XS–3XL) |
| Touca | 20 | Letra |
| Luvas | 30 | Letra |
| Máscara | 40 | Letra |
| Avental | 50 | Letra |
| Botas | 60 | Número (35–46) |
| Casaco | 70 | Letra |
| Calça | 80 | Número |

### Credenciais de acesso (demo)

| Nº | Password inicial | Função |
|---|---|---|
| 1077 | `teste` | Admin |
| Outros | nº mecanográfico | Varia |

---

## 11. Notas técnicas

### Estrutura de ficheiros

```
<pasta instalação>\
  PEPIDI.exe                  — aplicação principal
  PEPIDI.dll                  — lógica da aplicação
  AgentePEPIDI.exe            — agente de background (opcional)
  [dependências .dll]
  Setup\
    PEPIDI_Setup_Completo.sql — script SQL completo
    Manual_Instalacao_PEPIDI.md

%APPDATA%\PEPIDI\
  conn.bin                    — ligação BD (AES-256, criado no 1º arranque)
```

### Segurança

- Passwords guardadas como SHA-256 lowercase hex — nunca em texto claro
- Connection string encriptada em AES-256 no `conn.bin`
- Todas as alterações em tabelas auditadas via triggers com `SESSION_CONTEXT`

### Alterar password de um utilizador (SSMS)

```sql
-- Calcular o hash SHA-256 da nova password primeiro (PowerShell):
-- $bytes = [System.Text.Encoding]::UTF8.GetBytes("nova_pass")
-- $hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes)
-- ($hash | ForEach-Object { $_.ToString("x2") }) -join ""

EXEC sp_AlterarPassword @Nr = 2001, @NovaPasswordHash = '<hash_sha256>';
```

### Desinstalar

Painel de Controlo → Programas → PEPIDI → Desinstalar  
*(o `conn.bin` em `%APPDATA%\PEPIDI\` não é apagado automaticamente — apagar manualmente se necessário)*

---

*PEPIDI 0.5 · Estágio Curricular TeSP PSI · UA-ESTGA · 2026*
