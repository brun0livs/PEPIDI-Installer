<div align="center">

# PEPIDI+Installer

**Pacote de instalação do sistema PEPIDI 0.5**

*Diatosta – Indústria Alimentar, S.A. · 2024-2026*

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
![SQL Server](https://img.shields.io/badge/DB-SQL%20Server-CC2927?style=flat-square&logo=microsoftsqlserver)
![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D4?style=flat-square&logo=windows)

</div>

---

## 📦 Conteúdo deste repositório

| Ficheiro / Pasta | Descrição |
|---|---|
| `PEPIDI_0.5_Setup.exe` | Instalador Windows (23 MB) — sem necessidade de administrador |
| `PEPIDI_Setup_Completo.sql` | Script SQL completo: schema, stored procedures, triggers e dados demo |
| `Manual_Instalacao_PEPIDI.md` | Manual de instalação passo-a-passo em português |
| `Codigo/PEPIDI-0.5/` | Código-fonte da aplicação principal (.NET 9, WinForms) |
| `Codigo/AgentePEPIDI/` | Código-fonte do agente de notificações (.NET Framework 4.8) |

---

## 🚀 Instalação rápida

### 1. Base de dados

1. Abrir o **SQL Server Management Studio** e ligar ao servidor
2. `File → Open → File...` → selecionar `PEPIDI_Setup_Completo.sql`
3. Executar (`F5`)

O script cria a base de dados `PEPIDI` com todas as tabelas, stored procedures, triggers e dados demo.

### 2. Aplicação

Executar `PEPIDI_0.5_Setup.exe` e seguir o assistente.

- Instala em `%LOCALAPPDATA%\PEPIDI` — **não requer administrador**
- Detecta automaticamente se o .NET 9 Runtime está instalado

### 3. Primeiro login

| Campo | Valor |
|---|---|
| Nº mecanográfico | `1077` |
| Password | `teste` |

> ⚠️ Alterar a password após o primeiro acesso.

Para o manual completo: [`Manual_Instalacao_PEPIDI.md`](Manual_Instalacao_PEPIDI.md)

---

## 🔒 Notas de segurança

| Mecanismo | Implementação |
|---|---|
| Passwords | SHA-256 (UTF-8 → hex lowercase) — nunca guardadas em claro |
| Connection string | AES-256 encriptada em `%APPDATA%\PEPIDI\conn.bin` |
| Auditoria | 5 triggers que registam todas as alterações com o nº do utilizador (`SESSION_CONTEXT`) |
| Permissões | Controlo de acesso por perfil (Admin / RH / Encarregado / Operador) |
| Instalação | `PrivilegesRequired=lowest` — sem elevação de privilégios |

---

## 🛠️ Pré-requisitos

| Componente | Versão mínima |
|---|---|
| Windows | 10 (build 17763) / 11 |
| SQL Server | 2019+ (qualquer edição, incluindo Express) |
| .NET 9 Desktop Runtime | 9.x |
| .NET Framework 4.8 | 4.8 (para o AgentePEPIDI — já incluído no Windows 10/11) |

---

## 🔄 Reset da base de dados demo

Para repor o estado inicial (apagar dados operacionais, manter utilizador 1077 e dados de referência):

```sql
USE PEPIDI;
EXEC sp_ResetPEPIDI;
```

---

<div align="center">

Desenvolvido por **Bruno Oliveira** para **Diatosta – Indústria Alimentar, S.A.**

</div>
