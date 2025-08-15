using RestSharp;
using RestSharp.Authenticators;
using System.Formats.Asn1;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using TestIdeaCenterAPI.Models;


namespace TestIdeaCenterAPI
{
    public class IdeaCenterApiTests
    {
        private RestClient client;
        private const string baseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:84";
        private static string lastCreatedIdeaId;

        [OneTimeSetUp]
        public void Setup()
        {
        
        string jwtToken = GetJwtToken("emi123@example.com", "123456");

        var options = new RestClientOptions(baseUrl)
        {
            Authenticator = new JwtAuthenticator(jwtToken)
        };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody (new { email, password });            
            
            var response = loginClient.Execute(request);           
            var jsonItems = JsonSerializer.Deserialize<JsonElement>(response.Content);
            var accessToken = jsonItems.GetProperty("accessToken").ToString() ?? string.Empty;

            return accessToken;
        }

        [Test, Order(1)]
        public void Test_AddNewIdea_WithRequiedFields_ShouldReturnSuscces()
        {
            var requestBody = new IdeaDTO
            {
                Title = "Test",
                Url = "",
                Description = "Test Idea",
            };

            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(requestBody);
            var response = this.client.Execute(request);
           
            var jsonItems = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(jsonItems?.Msg, Is.EqualTo("Successfully created!"));
        }

        [Test, Order(2)]
        public void Test_GetAllIdeas_ShouldReturnSuscces()
        {
            var request = new RestRequest("/api/Idea/All", Method.Get);         
            var response = this.client.Execute(request);
            
            var jsonItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(jsonItems, Is.Not.Null, "Response array is null");
            Assert.That(jsonItems, Is.Not.Empty, "Response array is empty");
            Assert.That(jsonItems.Count, Is.GreaterThan(0), "Response array is empty");          

            lastCreatedIdeaId = jsonItems.LastOrDefault().IdeaId;
        }

        [Test, Order(3)]
        public void Test_EditTheLastCreatedIdea_ShouldReturnSuscces()
        {
            var request = new RestRequest("/api/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            request.AddJsonBody(new IdeaDTO
            {
                Title = "New title",
                Description = "Description",
                Url = ""
            });
            var response = this.client.Execute(request);

            var jsonItems = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(jsonItems.Msg, Is.EqualTo("Edited successfully"));     
        }

        [Test, Order(4)]
        public void Test_DeleteTheEditedIdea_ShouldReturnSuscces()
        {
            var request = new RestRequest("/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);

            var response = this.client.Execute(request);    

            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain ("The idea is deleted"));
        }

        [Test, Order(5)]
        public void Test_CreateIdea_WihtoutRequiredFields_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Idea/Create", Method.Post);

            request.AddJsonBody(new IdeaDTO
            {               
                Description = "Description",
                Url = ""
            });

            var response = this.client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));       
        }
        [Test, Order(6)]
        public void Test_EditNonExistingIdea_ShouldReturnBadRequest()
        {
            var fakeIdeaId = "123";
            var request = new RestRequest("/api/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaId", fakeIdeaId);

            request.AddJsonBody(new IdeaDTO
            {
                Title = "Edit non-existing idea",
                Description = "Description",
                Url = ""
            });

            var response = this.client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such idea!"));
        }

        [Test, Order(7)]
        public void Test_DeleteNonExistingIdea_ShouldReturnBadRequest()
        {
            var fakeIdeaId = "123";
            var request = new RestRequest("/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", fakeIdeaId);

            var response = this.client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such idea!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}