
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace TaskFlow.Tests.Integration;

public class OwnershipIntegrationTests
{
    private static async Task<string> RegisterAndLogin(
        HttpClient client, string email)
    {
        var register = await client.PostAsJsonAsync(
            "/api/users",
            new
            {
                name = "Integration User",
                email,
                password = "Password123!"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            register.StatusCode);

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email,
                password = "Password123!"
            });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        using var json = JsonDocument.Parse(
            await login.Content.ReadAsStringAsync());

        return json.RootElement
            .GetProperty("token")
            .GetString()!;
    }

    private static void Authenticate(
        HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private static async Task<Guid> CreateProject(
        HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/projects",
            new
            {
                name = "Projeto de integração",
                description = "Teste de propriedade"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        return json.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateTask(
        HttpClient client, Guid projectId)
    {
        var response = await client.PostAsJsonAsync(
            "/api/tasks",
            new
            {
                projectId,
                title = "Tarefa de integração",
                description = "Teste de segurança",
                priority = 2
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        return json.RootElement.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task UserCannotReadAnotherUsersProject()
    {
        using var factory = new TaskFlowApiFactory();
        using var owner = factory.CreateClient();
        using var other = factory.CreateClient();

        Authenticate(owner, await RegisterAndLogin(
            owner, "projectowner@example.com"));

        Authenticate(other, await RegisterAndLogin(
            other, "projectother@example.com"));

        var projectId = await CreateProject(owner);

        var response = await other.GetAsync(
            $"/api/projects/{projectId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UserCannotModifyAnotherUsersProject()
    {
        using var factory = new TaskFlowApiFactory();
        using var owner = factory.CreateClient();
        using var other = factory.CreateClient();

        Authenticate(owner, await RegisterAndLogin(
            owner, "modifyowner@example.com"));

        Authenticate(other, await RegisterAndLogin(
            other, "modifyother@example.com"));

        var projectId = await CreateProject(owner);

        var response = await other.PutAsJsonAsync(
            $"/api/projects/{projectId}",
            new
            {
                name = "Alteração não autorizada",
                description = "Não deve funcionar"
            });

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UserCannotReadAnotherUsersTask()
    {
        using var factory = new TaskFlowApiFactory();
        using var owner = factory.CreateClient();
        using var other = factory.CreateClient();

        Authenticate(owner, await RegisterAndLogin(
            owner, "taskowner@example.com"));

        Authenticate(other, await RegisterAndLogin(
            other, "taskother@example.com"));

        var projectId = await CreateProject(owner);
        var taskId = await CreateTask(owner, projectId);

        var response = await other.GetAsync(
            $"/api/tasks/{taskId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UserCannotModifyAnotherUsersTask()
    {
        using var factory = new TaskFlowApiFactory();
        using var owner = factory.CreateClient();
        using var other = factory.CreateClient();

        Authenticate(owner, await RegisterAndLogin(
            owner, "taskmodifyowner@example.com"));

        Authenticate(other, await RegisterAndLogin(
            other, "taskmodifyother@example.com"));

        var projectId = await CreateProject(owner);
        var taskId = await CreateTask(owner, projectId);

        var response = await other.PatchAsync(
            $"/api/tasks/{taskId}/start", null);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UserCannotCreateTaskInAnotherUsersProject()
    {
        using var factory = new TaskFlowApiFactory();
        using var owner = factory.CreateClient();
        using var other = factory.CreateClient();

        Authenticate(owner, await RegisterAndLogin(
            owner, "createowner@example.com"));

        Authenticate(other, await RegisterAndLogin(
            other, "createother@example.com"));

        var projectId = await CreateProject(owner);

        var response = await other.PostAsJsonAsync(
            "/api/tasks",
            new
            {
                projectId,
                title = "Tarefa indevida",
                description = "Não autorizada",
                priority = 2
            });

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedUser_CanCreateAndUpdateProjectAndTask()
    {
        using var factory = new TaskFlowApiFactory();
        using var client = factory.CreateClient();

        Authenticate(client, await RegisterAndLogin(
            client, "workflow@example.com"));

        var projectId = await CreateProject(client);

        var updateProject = await client.PutAsJsonAsync(
            $"/api/projects/{projectId}",
            new
            {
                name = "Projeto atualizado",
                description = "Atualizado com sucesso"
            });

        Assert.Equal(
            HttpStatusCode.OK,
            updateProject.StatusCode);

        var taskId = await CreateTask(client, projectId);

        var updateTask = await client.PutAsJsonAsync(
            $"/api/tasks/{taskId}",
            new
            {
                title = "Tarefa atualizada",
                description = "Descrição atualizada",
                priority = 3
            });

        Assert.Equal(
            HttpStatusCode.OK,
            updateTask.StatusCode);

        var start = await client.PatchAsync(
            $"/api/tasks/{taskId}/start", null);

        Assert.Equal(HttpStatusCode.OK, start.StatusCode);

        var complete = await client.PatchAsync(
            $"/api/tasks/{taskId}/complete", null);

        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);
    }
}
