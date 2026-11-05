# TaskFlow API

**API REST para gerenciamento de projetos e tarefas, desenvolvida com C# e ASP.NET Core (.NET 10).**

O TaskFlow utiliza arquitetura em camadas, Entity Framework Core, SQL Server e autenticação JWT, com foco em organização de código, segurança, regras de negócio e testes automatizados.

**Status do projeto:**

- **Backend: Concluído e validado localmente**
- **Testes automatizados: 126 aprovados**
- **Frontend Angular: Próxima etapa**
- **Deploy em produção: Pendente**

---

## Tecnologias utilizadas

- C# e .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- SQL Server
- JWT Bearer Authentication
- ASP.NET Core Identity PasswordHasher
- ASP.NET Core Rate Limiting
- ProblemDetails (RFC 9457)
- OpenAPI 3.1
- Swagger UI com autenticação JWT
- CORS
- HTTPS
- xUnit
- WebApplicationFactory
- EF Core InMemory para testes de integração
- Git e GitHub

## Arquitetura

O projeto utiliza uma arquitetura em camadas, separando responsabilidades e facilitando a manutenção, evolução e testabilidade da aplicação.

```text
TaskFlow/
├── TaskFlow.Api/
│   ├── Controllers/
│   ├── Exceptions/
│   ├── Helpers/
│   ├── OpenApi/
│   │   └── JwtSecurityDocumentTransformer.cs
│   ├── Properties/
│   ├── appsettings.json
│   └── Program.cs
│
├── TaskFlow.Application/
│   ├── DTOs/
│   └── Interfaces/
│
├── TaskFlow.Domain/
│
├── TaskFlow.Infrastructure/
│   ├── Data/
│   ├── Migrations/
│   └── Services/
│
└── xUnit/
    └── Integration/
```

### Responsabilidades das camadas

- **API:** endpoints HTTP, autenticação, autorização, validações, documentação OpenAPI e configuração dos middlewares.
- **Application:** contratos dos serviços e objetos de transferência de dados (DTOs).
- **Domain:** entidades e conceitos do domínio.
- **Infrastructure:** persistência, acesso ao banco de dados e implementação dos serviços.
- **Tests:** testes automatizados de regras de negócio, comportamento, segurança e integração.

---

## Funcionalidades implementadas

### Autenticação e usuários

- Cadastro de usuários.
- Armazenamento seguro de senhas por hash.
- Login com validação de credenciais.
- Emissão de tokens JWT com validade de uma hora.
- Validação de assinatura, emissor, audiência e expiração.
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
- Autorização e verificação de propriedade dos recursos.
- Validação de dados recebidos pela API.
- Respostas padronizadas com ProblemDetails.
- Tratamento global de exceções.
- Rate Limiting no endpoint de login.
- CORS configurável por ambiente.
- HTTPS validado no ambiente local.
- Configurações JWT armazenadas em User Secrets durante o desenvolvimento.

---

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

---

## Endpoints da API

### Autenticação

| Método | Endpoint | Descrição |
|---|---|---|
| POST | `/api/auth/login` | Autenticar usuário e gerar token JWT |

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

Os endpoints de projetos e tarefas exigem autenticação JWT.

O cadastro de usuários é público, enquanto a consulta e a desativação da conta exigem autenticação e verificação de identidade.

---

## Documentação OpenAPI e Swagger UI

A API possui documentação interativa utilizando OpenAPI e Swagger UI.

A interface permite visualizar os endpoints, consultar os modelos de requisição e resposta, executar chamadas HTTP e testar a autenticação JWT.

### Acessar o Swagger

Com a aplicação iniciada no perfil HTTPS de desenvolvimento:

**Swagger UI:**

```text
https://localhost:7291/swagger
```

**Documento OpenAPI:**

```text
https://localhost:7291/openapi/v1.json
```

A documentação é disponibilizada somente no ambiente de desenvolvimento.

### Autenticação pelo Swagger

1. Cadastre um usuário utilizando `POST /api/users`, caso necessário.
2. Execute `POST /api/auth/login`.
3. Copie o token JWT retornado.
4. Clique no botão **Authorize**.
5. Informe o token no campo disponível.
6. Execute os endpoints protegidos.

O endpoint de login é identificado como público na documentação, enquanto os endpoints protegidos apresentam o esquema de autenticação Bearer.

---

## CORS — Integração com Angular

O backend possui uma política CORS configurável por ambiente, permitindo a integração com o frontend Angular.

No ambiente local de desenvolvimento, a origem permitida é:

```text
http://localhost:4200
```

A política permite os métodos e cabeçalhos necessários às requisições da aplicação.

As origens permitidas são definidas pela configuração `Cors:AllowedOrigins`.

O arquivo `appsettings.json` mantém a lista de origens vazia por padrão, evitando liberar origens indiscriminadamente.

Em produção, será necessário configurar explicitamente o domínio do frontend.

---

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

Exemplo para desenvolvimento local com autenticação integrada do Windows:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TaskFlowDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

O exemplo utiliza uma configuração local de desenvolvimento. Para produção, revise as configurações de conexão, criptografia e certificados.

Evite armazenar credenciais reais em arquivos versionados.

### 3. Configurar JWT

Utilize User Secrets no projeto da API.

Na raiz do repositório, execute:

```powershell
dotnet user-secrets set "Jwt:Issuer" "TaskFlow.Api" --project TaskFlow.Api

dotnet user-secrets set "Jwt:Audience" "TaskFlow.Client" --project TaskFlow.Api

$key = [byte[]]::new(64)
[System.Security.Cryptography.RandomNumberGenerator]::Fill($key)

dotnet user-secrets set "Jwt:Key" ([Convert]::ToBase64String($key)) --project TaskFlow.Api
```

A chave JWT deve ser mantida em segredo e não deve ser publicada no GitHub.

Em produção, utilize variáveis de ambiente ou um serviço apropriado de gerenciamento de segredos.

### 4. Aplicar as migrations

No Console do Gerenciador de Pacotes do Visual Studio:

```powershell
Update-Database -Project TaskFlow.Infrastructure -StartupProject TaskFlow.Api
```

### 5. Configurar o certificado HTTPS

```powershell
dotnet dev-certs https --trust
```

### 6. Executar a API

Na raiz do repositório:

```powershell
dotnet run --project TaskFlow.Api --launch-profile https
```

Endereços locais configurados:

```text
HTTPS: https://localhost:7291
HTTP:  http://localhost:5133
Swagger: https://localhost:7291/swagger
```

As portas podem ser alteradas conforme o ambiente.

---

## Testes automatizados

O TaskFlow utiliza **xUnit** para validar funcionalidades, regras de negócio e comportamentos da API.

A suíte contempla cenários relacionados a:

- Autenticação JWT.
- Autorização e permissões.
- Controle de acesso por usuário.
- Operações com projetos e tarefas.
- Validação de dados.
- Tratamento de erros HTTP.
- Rate Limiting.
- CORS.
- Testes de integração dos controllers e serviços.

Os testes de integração utilizam `WebApplicationFactory` e EF Core InMemory para executar cenários isolados.

### Executar todos os testes

```powershell
dotnet test
```

### Resultado da última validação

**Data:** 05/11/2026

| Indicador | Resultado |
|---|---|
| Testes executados | 126 |
| Testes aprovados | 126 |
| Falhas | 0 |
| Testes ignorados | 0 |
| Compilação | Aprovada |

**Resultado: 126 testes aprovados — 0 falhas — 0 ignorados.**

A execução foi realizada localmente com .NET 10.

---

## Status e evolução do projeto

### Backend — Concluído

- [x] Estruturar a solução em camadas.
- [x] Implementar persistência com EF Core e SQL Server.
- [x] Implementar cadastro e autenticação de usuários.
- [x] Implementar autenticação JWT.
- [x] Proteger endpoints de projetos e tarefas.
- [x] Implementar controle de acesso por usuário.
- [x] Implementar gerenciamento de projetos.
- [x] Implementar gerenciamento de tarefas.
- [x] Padronizar respostas de erro com ProblemDetails.
- [x] Implementar tratamento global de exceções.
- [x] Adicionar Rate Limiting ao login.
- [x] Configurar CORS para integração com Angular.
- [x] Configurar documentação OpenAPI e Swagger UI.
- [x] Integrar autenticação JWT ao Swagger.
- [x] Validar HTTPS no ambiente local.
- [x] Revisar configurações de desenvolvimento e segurança.
- [x] Executar validação final com 126 testes aprovados.

### Frontend e publicação — Próximas etapas

- [ ] Criar o frontend com Angular.
- [ ] Implementar tela de login.
- [ ] Implementar dashboard.
- [ ] Criar interfaces de gerenciamento de projetos.
- [ ] Criar interfaces de gerenciamento de tarefas.
- [ ] Integrar frontend e backend.
- [ ] Configurar o ambiente de produção.
- [ ] Publicar a aplicação completa.

> **Nota:** o desenvolvimento e a validação local do backend foram concluídos. O projeto ainda não passou pela implantação e validação em ambiente de produção. Configurações de infraestrutura, certificados, segredos, banco de dados e monitoramento deverão ser revisadas antes da exposição pública.

---

## Objetivo do projeto

O TaskFlow é um projeto de portfólio voltado ao desenvolvimento backend com .NET, demonstrando conhecimentos em:

- Desenvolvimento de APIs REST.
- Arquitetura de software e separação de responsabilidades.
- Persistência com Entity Framework Core.
- Modelagem de entidades e regras de negócio.
- Autenticação e autorização.
- Segurança de aplicações.
- Testes unitários e de integração.
- Documentação de APIs.
- Versionamento com Git e GitHub.

O próximo objetivo é transformar a API em uma aplicação web completa, utilizando Angular no frontend.

---

## Autor

**Rodrigo Abade Nunes**

Desenvolvedor Backend .NET

[GitHub](https://github.com/RodrigoAbade) | [Portfólio](https://rodrigo-abade.vercel.app)
