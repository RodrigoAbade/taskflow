# TaskFlow API

API REST para gerenciamento de projetos e tarefas, desenvolvida em **C# e ASP.NET Core (.NET 10)**, com foco em arquitetura em camadas, regras de negócio, persistência com Entity Framework Core e autenticação JWT.

> **Status:** em desenvolvimento. A autenticação JWT já funciona para os endpoints de projetos; a proteção dos endpoints de tarefas e usuários ainda está em andamento.

## Tecnologias

- C# / .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10 e SQL Server
- JWT Bearer Authentication
- ASP.NET Core Identity PasswordHasher (hash e verificação de senhas)
- xUnit (testes automatizados)
- OpenAPI
- Git e GitHub

## Arquitetura

A solução é dividida em quatro projetos:

```text
TaskFlow/
├── TaskFlow.Api/             # Controllers, autenticação e configuração da API
├── TaskFlow.Application/     # DTOs e interfaces dos serviços
├── TaskFlow.Domain/          # Entidades, enums e regras de negócio
├── TaskFlow.Infrastructure/  # EF Core, configurações, migrations e serviços
└── TaskFlow.Tests/           # Testes automatizados
```

## Funcionalidades implementadas

- **Projetos:** criar, listar, consultar por ID, atualizar e desativar (soft delete).
- **Tarefas:** criar, listar, consultar por ID ou projeto, atualizar, iniciar, concluir e cancelar.
- **Usuários:** cadastrar, listar, consultar e desativar.
- **Autenticação:** cadastro com senha armazenada em hash, login com validação de credenciais e emissão de JWT com expiração de 1 hora.
- **Autorização:** endpoints de projetos protegidos por `[Authorize]`.
- **Persistência:** SQL Server com Entity Framework Core e migrations.

### Exemplos de endpoints

| Método | Endpoint | Descrição |
| --- | --- | --- |
| POST | `/api/auth/login` | Autenticar e obter JWT |
| POST | `/api/users` | Cadastrar usuário |
| GET | `/api/projects` | Listar projetos (JWT obrigatório) |
| POST | `/api/projects` | Criar projeto (JWT obrigatório) |
| GET | `/api/tasks` | Listar tarefas |
| GET | `/api/tasks/project/{projectId}` | Listar tarefas de um projeto |
| PATCH | `/api/tasks/{id}/start` | Iniciar tarefa |
| PATCH | `/api/tasks/{id}/complete` | Concluir tarefa |
| PATCH | `/api/tasks/{id}/cancel` | Cancelar tarefa |

> A proteção JWT dos endpoints de tarefas e usuários está pendente. **Não publique a API na internet como serviço de produção neste estágio.**

## Como executar localmente

### Pré-requisitos

- SDK do .NET 10
- SQL Server
- Visual Studio 2026 ou editor compatível

1. Clone o repositório:

   ```bash
   git clone https://github.com/RodrigoAbade/taskflow.git
   cd taskflow
   ```

2. Configure a connection string `ConnectionStrings:DefaultConnection` para sua instância local do SQL Server, preferencialmente em User Secrets ou configuração local não versionada.

3. Configure os segredos JWT no projeto `TaskFlow.Api`:

   ```powershell
   cd TaskFlow.Api
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:Issuer" "TaskFlow.Api"
   dotnet user-secrets set "Jwt:Audience" "TaskFlow.Client"

   $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
   $bytes = New-Object byte[] 64
   $rng.GetBytes($bytes)
   $rng.Dispose()
   dotnet user-secrets set "Jwt:Key" ([Convert]::ToBase64String($bytes))
   cd ..
   ```

4. Aplique as migrations com as ferramentas do EF Core, usando `TaskFlow.Infrastructure` como projeto de migrations e `TaskFlow.Api` como projeto de inicialização. No Console do Gerenciador de Pacotes do Visual Studio:

   ```powershell
   Update-Database -Project TaskFlow.Infrastructure -StartupProject TaskFlow.Api
   ```

5. Execute a API:

   ```bash
   dotnet run --project TaskFlow.Api
   ```

A URL HTTPS local depende do perfil de inicialização. O arquivo `TaskFlow.Api/TaskFlow.Api.http` contém exemplos de requisições; use apenas credenciais de teste locais e nunca versione tokens reais.

## Testes

Execute os testes automatizados:

```bash
dotnet test
```

## Próximos passos

- [ ] Proteger endpoints de tarefas e usuários com autorização adequada
- [ ] Definir permissões por usuário e propriedade dos recursos
- [ ] Ampliar testes automatizados e cenários de autenticação
- [ ] Melhorar validações, respostas de erro e segurança da API
- [ ] Revisar documentação e preparar deploy

## Autor

**Rodrigo Abade Nunes**  
[GitHub](https://github.com/RodrigoAbade)
