using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PostsAPI.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PostsAPI.Test;

[TestClass]
public sealed class PostsApiTests
{
    private static ApiFactory _factory = null!;
    private static readonly WebApplicationFactoryClientOptions Https = new() { BaseAddress = new Uri("https://localhost") };
    private static void GuardTestDatabase(AppDbContext db)
    {
        var name = db.Database.GetDbConnection().Database;
        if (!name.EndsWith("TestDb"))
            throw new InvalidOperationException($"Refusing to touch '{name}'");
    }
    private record TokenResposne(string Token);
    private async Task<HttpClient> LoggedInClientAsync()
    {
        var client = _factory.CreateClient(Https);
        var credentials = new { email = $"{Guid.NewGuid()}@test.com", password = "Password1234!" };
        var response = await client.PostAsJsonAsync("/api/auth/register", credentials);
        response.EnsureSuccessStatusCode();
        var login = await client.PostAsJsonAsync("/api/auth/login", credentials);
        login.EnsureSuccessStatusCode();
        var content = await login.Content.ReadFromJsonAsync<TokenResposne>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content!.Token);
        return client;
    }
    private async Task<HttpClient> LoggedInAdminClientAsync()
    {
        var client = _factory.CreateClient(Https);
        var credentials = new { email = "admin@test.com", password = "admin1234" };
        var login = await client.PostAsJsonAsync("/api/auth/login", credentials);
        login.EnsureSuccessStatusCode();
        var content = await login.Content.ReadFromJsonAsync<TokenResposne>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content!.Token);
        return client;
    }

    [ClassInitialize]
    public static void Init(TestContext _)
    {
        _factory = new ApiFactory();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        GuardTestDatabase(db);
        db.Database.EnsureDeleted();
        db.Database.Migrate();
        PasswordHasher<User> hasher = new();
        User admin = new User { Email = "admin@test.com", Role = "Admin" };
        admin.PasswordHash = hasher.HashPassword(admin, "admin1234");
        db.Users.Add(admin);
        db.SaveChanges();
    }

    [ClassCleanup]
    public static void CleanUp() => _factory.Dispose();

    [TestMethod]
    public void Tests_UseTheTestDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.AreEqual("PostsApiTestDb", db.Database.GetDbConnection().Database);
    }

    [TestMethod]
    public async Task GetAllPosts_ReturnsOk()
    {
        var response = await _factory.CreateClient(Https).GetAsync("/api/posts");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<GetAllPostsResponse>();
        Assert.IsGreaterThanOrEqualTo(0, content!.LastPage);
    }

    [TestMethod]
    public async Task CreatePost_WithoutToken_Returns401()
    {
        var response = await _factory.CreateClient(Https).PostAsJsonAsync("/api/posts", new {title = "Test Post Title", content = "Test Post Content"});
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task RegisterAndLogin_ReturnsToken()
    {
        var client = await LoggedInClientAsync();
        var response = await client.GetAsync("/api/auth/me");
        response.EnsureSuccessStatusCode();
    }
    private record CreatePostResponse(int Id);
    [TestMethod]
    public async Task RegisteredUser_CreatePost_Succeed()
    {
        var client = await LoggedInClientAsync();
        var testingPost = new { Title = "Test Post Title", Content = "Test Post Content" };
        var response = await client.PostAsJsonAsync("/api/posts", testingPost);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<CreatePostResponse>();
        var id = content!.Id;
        response = await client.GetAsync($"/api/posts/{id}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var post = await response.Content.ReadFromJsonAsync<PostResponse>();
        Assert.AreEqual(testingPost.Title, post!.Title);
        Assert.AreEqual(testingPost.Content, post!.Content);
    }
    [TestMethod]
    public async Task UnregisteredUser_Comment_Returns401()
    {
        var response = await _factory.CreateClient(Https).PostAsJsonAsync($"/api/posts/1/comments", new { Content = "Test Post Comment Unauthenticated" });
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [TestMethod]
    public async Task RegisteredUser_Comment_ReturnsCreated()
    {
        var client = await LoggedInClientAsync();
        var testingPost = new { Title = "Test Post Title 2", Content = "Test Post Content 2" };
        var response = await client.PostAsJsonAsync("/api/posts", testingPost);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<CreatePostResponse>();
        var id = content!.Id;
        var testingComment = new { Content = "Test Post Comment 1" };
        response = await client.PostAsJsonAsync($"/api/posts/{id}/comments", testingComment);
        var comment = await response.Content.ReadFromJsonAsync<CommentResponse>();
        Assert.AreEqual(testingComment.Content, comment!.Content);
    }
    private record GetPostsResponse(IEnumerable<PostResponse> Posts);
    [TestMethod]
    public async Task CreatedComment_Appears_InGetPosts()
    {
        var client = await LoggedInClientAsync();
        var testingPost = new { Title = "Test Post Title 3", Content = "Test Post Content 3" };
        var response = await client.PostAsJsonAsync("/api/posts", testingPost);
        var content = await response.Content.ReadFromJsonAsync<CreatePostResponse>();
        var id = content!.Id;
        var testingComment = new { Content = "Test Post Comment 2" };
        response = await client.PostAsJsonAsync($"/api/posts/{id}/comments", testingComment);
        response = await client.GetAsync("/api/posts");
        var posts = (await response.Content.ReadFromJsonAsync<GetPostsResponse>())!.Posts;
        var foundComment = posts!.FirstOrDefault(p => p.Comments.Any(c => c.Content == testingComment.Content));
        Assert.IsNotNull(foundComment);
    }
    [TestMethod]
    public async Task PostNonOwner_UpdatePost_Returns403()
    {
        var client = await LoggedInClientAsync();
        var testingPost = new { Title = "Test Post Title 4", Content = "Test Post Content 4" };
        var response = await client.PostAsJsonAsync("/api/posts", testingPost);
        var content = await response.Content.ReadFromJsonAsync<CreatePostResponse>();
        var id = content!.Id;
        var nonOwnerClient = await LoggedInClientAsync();
        var testingModifiedPost = new { Title = "Test Post Title Modified" , Content = "Test Post Content Modified"};
        response = await nonOwnerClient.PutAsJsonAsync($"/api/posts/{id}", testingModifiedPost);
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        response = await client.PutAsJsonAsync($"/api/posts/{id}", testingModifiedPost);
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }
    [TestMethod]
    public async Task NonAdmin_AddTag_Returns403()
    {
        var client = await LoggedInClientAsync();
        var testingTag = new { Name = "DANGEROUS" };
        var response = await client.PostAsJsonAsync("/api/tags", testingTag);
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task Update_MissingPost_Returns_404()
    {
        var client = await LoggedInClientAsync();
        var testingPost = new { Title = "Test Post Title 5", Content = "Test Post Content 5" };
        var response = await client.PutAsJsonAsync("/api/posts/5184", testingPost);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);   
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.IsFalse(string.IsNullOrEmpty(problemDetails!.Title));
    }

    [TestMethod]
    public async Task PostNonOwner_DeletePost_Returns403()
    {
        var client = await LoggedInClientAsync();
        var testingPost = new { Title = "Test Post Title 6", Content = "Test Post Content 6" };
        var response = await client.PostAsJsonAsync("/api/posts", testingPost);
        var content = await response.Content.ReadFromJsonAsync<CreatePostResponse>();
        var id = content!.Id;
        var nonOwnerClient = await LoggedInClientAsync();
        response = await nonOwnerClient.DeleteAsync($"/api/posts/{id}");
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task Admin_AddTag_ReturnsCreated()
    {
        var client = await LoggedInAdminClientAsync();
        var testingTag = new { Name = "Test Tag 1" };
        var response = await client.PostAsJsonAsync("/api/tags", testingTag);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }
    [TestMethod]
    public async Task Admin_AddDuplicateTag_Returns409()
    {
        var client = await LoggedInAdminClientAsync();
        var testingTag = new { Name = "Test Tag 2" };
        await client.PostAsJsonAsync("/api/tags", testingTag);
        var response = await client.PostAsJsonAsync("/api/tags", testingTag);
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }
    [TestMethod]
    public async Task AttachTag_ByNonOwner_Returns403()
    {
        var adminClient = await LoggedInAdminClientAsync();
        var testingTag = new { Name = "Test Tag 3" };
        var response = await adminClient.PostAsJsonAsync("/api/tags", testingTag);
        var tagContent = await response.Content.ReadFromJsonAsync<Tag>();
        int tagId = tagContent!.Id;
        var ownerClient = await LoggedInClientAsync();
        var testingPost = new { Title = "Test Post Title 7", Content = "Test Post Content 7" };
        response = await ownerClient.PostAsJsonAsync("/api/posts", testingPost);
        var postContent = await response.Content.ReadFromJsonAsync<CreatePostResponse>();
        int postId = postContent!.Id;
        var nonOwnerClient = await LoggedInClientAsync();
        response = await nonOwnerClient.PutAsync($"/api/posts/{postId}/tags/{tagId}", null);
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
    [TestMethod]
    public async Task Concurrent_50Likes_UpdatesSuccessfully()
    {
        var client = await LoggedInClientAsync();
        var testingPost = new { Title = "Test Post Title 8", Content = "Test Post Content 8" };
        var response = await client.PostAsJsonAsync("/api/posts", testingPost);
        var postContent = await response.Content.ReadFromJsonAsync<CreatePostResponse>();
        int postId = postContent!.Id;
        var requests = Enumerable.Range(0, 50)
    .Select(_ => client.PostAsync($"/api/posts/{postId}/like", null));
        var responses = await Task.WhenAll(requests);
        Assert.IsTrue(responses.All(r => r.IsSuccessStatusCode));

        response = await client.GetAsync($"/api/posts/{postId}");
        var post = await response.Content.ReadFromJsonAsync<PostResponse>();
        Assert.AreEqual(50, post!.Likes);
    }
}
