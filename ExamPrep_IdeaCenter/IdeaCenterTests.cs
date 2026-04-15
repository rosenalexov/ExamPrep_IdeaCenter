using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using JsonElement = System.Text.Json.JsonElement;
using ExamPrep_IdeaCenter.DTOs;

namespace ExamPrep_IdeaCenter;

public class Tests
{
    private RestClient client;
    private static string lastCreatedIdeaId;
    
    private const string BaseUrl = "http://144.91.123.158:82";
    private const string StaticToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiJjZWNhMjRhNy0yYjU0LTQzNWUtODEyZS1iZDI2YjgyZDcxOWIiLCJpYXQiOiIwNC8xNS8yMDI2IDE1OjM2OjE0IiwiVXNlcklkIjoiNjNiODI0YzMtZDQ1OS00NmFjLTUzNjMtMDhkZTc2YTJkM2VjIiwiRW1haWwiOiJUZXN0Um9zc1Rlc3RAdGVzdC5jb20iLCJVc2VyTmFtZSI6IlRlc3RSb3NzVGVzdCIsImV4cCI6MTc3NjI4ODk3NCwiaXNzIjoiSWRlYUNlbnRlcl9BcHBfU29mdFVuaSIsImF1ZCI6IklkZWFDZW50ZXJfV2ViQVBJX1NvZnRVbmkifQ.0RQAZ8Gabmo6ilnqVxOAWmtja7uNdQKTH0wAYv9X-5E";
    private const string Email = "testrosstest@test.com";
    private const string Password = "123456";
    
    [OneTimeSetUp]
    public void Setup()
    {
        string jwtToken = !string.IsNullOrWhiteSpace(StaticToken) 
            ? StaticToken 
            : GetJwtToken(Email, Password);

        RestClientOptions options = new RestClientOptions(BaseUrl)
        {
            Authenticator = new JwtAuthenticator(jwtToken)
        };
        
        client = new RestClient(options);
    }

    private static string GetJwtToken(string email, string password)
    {
        RestClient tempClient = new RestClient(BaseUrl);
        RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
        request.AddJsonBody(new {  email, password });
        
        RestResponse response = tempClient.Execute(request);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            JsonElement content = JsonSerializer.Deserialize<JsonElement>(response.Content!);
            string? token = content.GetProperty("accessToken").GetString();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("Invalid token");
            }
            
            return token;
        }
        else
        {
            throw new InvalidOperationException("Failed to authenticate");
        }
    }

    [Order(1)]
    [Test]
    public void CreateIdea_WithRequiredFields_ShouldSucceed()
    {
        //Arrange
        IdeaDto ideaData = new IdeaDto()
        {
            Title = "Test idea",
            Description = "Test idea description",
            Url = ""
        };

        RestRequest request = new RestRequest("/api/Idea/Create", Method.Post);
        request.AddJsonBody(ideaData);

        //Act
        RestResponse response = client.Execute(request);
        ApiResponseDto responseDto = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK");
        Assert.That(responseDto?.Msg, Is.EqualTo("Successfully created!"));
    }
    
    [Order(2)]
    [Test]
    public void GetAllIdeas_ShouldSucceed()
    {
        //Arrange
        RestRequest request = new RestRequest("/api/Idea/All", Method.Get);
        
        //Act
        RestResponse response = client.Execute(request);
        List<ApiResponseDto> responseDto = JsonSerializer.Deserialize<List<ApiResponseDto>>(response.Content);
        
        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK");
        Assert.That(responseDto, Is.Not.Null.Or.Empty);

        lastCreatedIdeaId = responseDto.LastOrDefault().IdeaId;
    }
    
    [Order(3)]
    [Test]
    public void EditIdea_ShouldSucceed()
    {
        //Arrange
        IdeaDto editData = new IdeaDto()
        {
            Title = "Edited test idea title",
            Description = "Edited test idea description",
            Url = ""
        };
        
        RestRequest request = new RestRequest("/api/Idea/Edit", Method.Put);
        request.AddQueryParameter("ideaId", lastCreatedIdeaId);
        request.AddJsonBody(editData);

        //Act
        RestResponse response = client.Execute(request);
        ApiResponseDto responseDto = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK");
        Assert.That(responseDto?.Msg, Is.EqualTo("Edited successfully"));
    }
    
    [Order(4)]
    [Test]
    public void DeleteIdea_ShouldSucceed()
    {
        //Arrange
        RestRequest request = new RestRequest("/api/Idea/Delete", Method.Delete);
        request.AddQueryParameter("ideaId", lastCreatedIdeaId);

        //Act
        RestResponse response = client.Execute(request);

        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK");
        Assert.That(response.Content, Is.EqualTo("\"The idea is deleted!\""));
    }
    
    [Order(5)]
    [Test]
    public void CreateIdea_WithoutAllRequiredFields_ShouldFail()
    {
        //Arrange
        IdeaDto ideaData = new IdeaDto()
        {
            Title = "",
            Description = "",
            Url = ""
        };

        RestRequest request = new RestRequest("/api/Idea/Create", Method.Post);
        request.AddJsonBody(ideaData);

        //Act
        RestResponse response = client.Execute(request);

        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400");
    }
    
    [Order(6)]
    [Test]
    public void EditNotExistingIdea_ShouldFail()
    {
        //Arrange
        IdeaDto editData = new IdeaDto()
        {
            Title = "Edited test idea title",
            Description = "Edited test idea description",
            Url = ""
        };
        
        RestRequest request = new RestRequest("/api/Idea/Edit", Method.Put);
        request.AddQueryParameter("ideaId", "123456");
        request.AddJsonBody(editData);

        //Act
        RestResponse response = client.Execute(request);

        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400");
        Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
    }
    
    [Order(7)]
    [Test]
    public void DeleteNotExistingIdea_ShouldReturn()
    {
        //Arrange
        RestRequest request = new RestRequest("/api/Idea/Delete", Method.Delete);
        request.AddQueryParameter("ideaId", "123456");

        //Act
        RestResponse response = client.Execute(request);

        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400");
        Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
        
    }

    [OneTimeTearDown]
    public void Teardown()
    {
        client?.Dispose();
    }
}