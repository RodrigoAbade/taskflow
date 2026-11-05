# TaskFlow API

API REST para gerenciamento de projetos e tarefas, desenvolvida com **C# e ASP.NET Core (.NET 10)**, utilizando arquitetura em camadas, Entity Framework Core, SQL Server e autenticação JWT.

O projeto tem como objetivo aplicar boas práticas de desenvolvimento backend, organização de código, segurança, regras de negócio e testes automatizados.

**Status:** Backend em desenvolvimento avançado — 122 testes automatizados aprovados.

## Tecnologias utilizadas

- C# e .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- SQL Server
- JWT Bearer Authentication
- ASP.NET Core Identity PasswordHasher
- ASP.NET Core Rate Limiting
- ProblemDetails (RFC 7807 / RFC 9457)
- OpenAPI
- xUnit
- EF Core InMemory para testes de integração
- Git e GitHub

## Arquitetura

O projeto utiliza uma arquitetura em camadas, com separação de responsabilidades.

```text
TaskFlow/
├── TaskFlow.Api/
│   ├── Controllers/
│   ├── Exceptions/
│   ├── Helpers/
│   └── Program.cs
├── TaskFlow.Application/
│   ├── DTOs/
│   └── Interfaces/
├── TaskFlow.Domain/
├── TaskFlow.Infrastructure/
│   ├── Data/
│   ├── Migrations/
│   └── Services/
└── xUnit/
    └── Integration/
```

### Responsabilidades das camadas

- **API:** endpoints HTTP, autenticação, autorização, validações e configuração dos middlewares.
- **Application:** contratos dos serviços e objetos de transferência de dados (DTOs).
- **Domain:** entidades e conceitos do domínio.
- **Infrastructure:** persistência, acesso ao banco de dados e implementação dos serviços.
- **Tests:** testes automatizados de comportamento, regras de negócio e integração.

## Funcionalidades implementadas

### Autenticação e usuários

- Cadastro de usuários.
- Armazenamento seguro de senhas por hash.
- Login com validação de credenciais.
- Emissão de tokens JWT com validade de uma hora.
- Validação de assinatura, emissor, audiência e expiração do token.
- Verificação de usuários ativos durante a autenticação.
- Consulta e desativação da própria conta.
- Controle de acesso a recursos de outros usuários.

### Gerenciamento de projetos

- Criação de projetos.
- Listagem de projetos do usuário autenticado.
- Consulta de projetos por identificador.
- Atualização de projetos.
- Desativação lógica de projetos (soft delete).
- Isolamento dos projetos por usuário.

### Gerenciamento de tarefas

- Criação de tarefas vinculadas a projetos.
- Listagem de tarefas.
- Consulta por identificador.
- Listagem por projeto.
- Atualização de tarefas.
- Início, conclusão e cancelamento.
- Validação de transições de estado.
- Controle de acesso baseado no usuário autenticado.

### Segurança e tratamento de erros

- Autenticação JWT nos endpoints protegidos.
- Verificação de propriedade dos recursos.
- Respostas padronizadas com ProblemDetails.
- Tratamento global de exceções.
- Validação de dados recebidos pela API.
- Rate Limiting no endpoint de login.

## Rate Limiting

O endpoint de autenticação possui proteção contra excesso de tentativas.

| Configuração | Valor |
|---|---|
| Endpoint | `POST /api/auth/login` |
| Limite | 5 requisições |
| Intervalo | 1 minuto |
| Identificação | Endereço IP |
| Limite excedido | HTTP 429 |
| Formato do erro | `application/problem+json` |

A política utiliza uma janela fixa de tempo (*Fixed Window Rate Limiter*).

Essa proteção ajuda a reduzir tentativas automatizadas excessivas de autenticação, mas não substitui outras medidas de segurança.

## Endpoints principais

### Autenticação

| Método | Endpoint | Descrição |
|---|---|---|
| POST | `/api/auth/login` | Autenticar usuário |

### Usuários

| Método | Endpoint | Descrição |
|---|---|---|
| POST | `/api/users` | Cadastrar usuário |
| GET | `/api/users/{id}` | Consultar a própria conta |
| DELETE | `/api/users/{id}` | Desativar a própria conta |

### Projetos

| Método | Endpoint | Descrição |
|---|---|---|
| POST | `/api/projects` | Criar projeto |
| GET | `/api/projects` | Listar projetos |
| GET | `/api/projects/{id}` | Consultar projeto |
| PUT | `/api/projects/{id}` | Atualizar projeto |
| DELETE | `/api/projects/{id}` | Desativar projeto |

### Tarefas

| Método | Endpoint | Descrição |
|---|---|---|
| POST | `/api/tasks` | Criar tarefa |
| GET | `/api/tasks` | Listar tarefas |
| GET | `/api/tasks/{id}` | Consultar tarefa |
| GET | `/api/tasks/project/{projectId}` | Listar tarefas por projeto |
| PUT | `/api/tasks/{id}` | Atualizar tarefa |
| PATCH | `/api/tasks/{id}/start` | Iniciar tarefa |
| PATCH | `/api/tasks/{id}/complete` | Concluir tarefa |
| PATCH | `/api/tasks/{id}/cancel` | Cancelar tarefa |

Os endpoints de projetos e tarefas exigem autenticação JWT. No gerenciamento de usuários, o cadastro é público, enquanto a consulta e a desativação exigem autenticação e verificação de identidade.

## Como executar localmente

### Pré-requisitos

- SDK do .NET 10
- SQL Server
- Visual Studio ou VS Code
- Git

### 1. Clonar o repositório

```bash
git clone https://github.com/RodrigoAbade/taskflow.git
cd taskflow
```

### 2. Configurar o banco de dados

Configure `ConnectionStrings:DefaultConnection` com uma conexão válida para sua instância do SQL Server.

Evite armazenar credenciais reais em arquivos versionados.

### 3. Configurar JWT

Utilize User Secrets no projeto da API:

```powershell
cd TaskFlow.Api

dotnet user-secrets set "Jwt:Issuer" "TaskFlow.Api"
dotnet user-secrets set "Jwt:Audience" "TaskFlow.Client"

$key = [byte[]]::new(64)
[System.Security.Cryptography.RandomNumberGenerator]::Fill($key)

dotnet user-secrets set "Jwt:Key" ([Convert]::ToBase64String($key))

cd ..
```

A chave JWT deve ser mantida em segredo.

### 4. Aplicar as migrations

No Console do Gerenciador de Pacotes do Visual Studio:

```powershell
Update-Database -Project TaskFlow.Infrastructure -StartupProject TaskFlow.Api
```

### 5. Executar a API

```powershell
dotnet run --project TaskFlow.Api
```

O endereço local será exibido no terminal.

Em ambiente de desenvolvimento, a aplicação também disponibiliza o documento OpenAPI em `/openapi/v1.json`.

## Testes automatizados

O projeto utiliza **xUnit** para validar funcionalidades e comportamentos da API.

Os testes abrangem regras de negócio, autenticação, autorização, respostas HTTP e integração.

Os testes de integração utilizam `WebApplicationFactory` e banco de dados EF Core InMemory para executar cenários isolados.

Para executar a suíte:

```powershell
dotnet test
```

### Resultado da última execução

**122 testes aprovados — 0 falhas — 0 ignorados.**

Esse resultado corresponde à execução local realizada em 05/11/2026.

## Próximas etapas

- [x] Implementar autenticação JWT.
- [x] Proteger endpoints de projetos e tarefas.
- [x] Implementar controle de acesso por usuário.
- [x] Padronizar respostas de erro.
- [x] Implementar tratamento global de exceções.
- [x] Adicionar Rate Limiting ao login.
- [x] Implementar testes automatizados.
- [ ] Configurar CORS para integração com Angular.
- [ ] Revisar e aprimorar a documentação OpenAPI.
- [ ] Revisar configurações de segurança e produção.
- [ ] Executar a validação final do backend.
- [ ] Desenvolver o frontend em Angular.
- [ ] Integrar frontend e backend.
- [ ] Preparar o deploy da aplicação completa.

> O backend ainda está em desenvolvimento e não deve ser considerado pronto para exposição pública em produção sem a revisão final de segurança e configuração.

## Objetivo do projeto

O TaskFlow é um projeto de portfólio voltado ao desenvolvimento backend com .NET, demonstrando conhecimentos em:

- Desenvolvimento de APIs REST.
- Arquitetura e separação de responsabilidades.
- Persistência com Entity Framework Core.
- Autenticação e autorização.
- Implementação de regras de negócio.
- Segurança de aplicações.
- Testes unitários e de integração.
- Versionamento com Git.

## Autor

**Rodrigo Abade Nunes**

Desenvolvedor Backend .NET

[GitHub](https://github.com/RodrigoAbade) | [Portfólio](https://rodrigo-abade.vercel.app)
