namespace TrybeHotel.Test;
using TrybeHotel.Dto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using TrybeHotel.Models;
using TrybeHotel.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Diagnostics;
using System.Xml;
using System.IO;



public class IntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    public HttpClient _clientTest;

    public IntegrationTest(WebApplicationFactory<Program> factory)
    {
        //_factory = factory;
        _clientTest = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TrybeHotelContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ContextTest>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryTestDatabase");
                });
                services.AddScoped<ITrybeHotelContext, ContextTest>();
                services.AddScoped<ICityRepository, CityRepository>();
                services.AddScoped<IHotelRepository, HotelRepository>();
                services.AddScoped<IRoomRepository, RoomRepository>();
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                using (var appContext = scope.ServiceProvider.GetRequiredService<ContextTest>())
                {
                    appContext.Database.EnsureCreated();
                    appContext.Database.EnsureDeleted();
                    appContext.Database.EnsureCreated();
                    appContext.Cities.Add(new City { CityId = 1, Name = "Manaus" });
                    appContext.Cities.Add(new City { CityId = 2, Name = "Palmas" });
                    appContext.SaveChanges();
                    appContext.Hotels.Add(new Hotel { HotelId = 1, Name = "Trybe Hotel Manaus", Address = "Address 1", CityId = 1 });
                    appContext.Hotels.Add(new Hotel { HotelId = 2, Name = "Trybe Hotel Palmas", Address = "Address 2", CityId = 2 });
                    appContext.Hotels.Add(new Hotel { HotelId = 3, Name = "Trybe Hotel Ponta Negra", Address = "Addres 3", CityId = 1 });
                    appContext.SaveChanges();
                    appContext.Rooms.Add(new Room { RoomId = 1, Name = "Room 1", Capacity = 2, Image = "Image 1", HotelId = 1 });
                    appContext.Rooms.Add(new Room { RoomId = 2, Name = "Room 2", Capacity = 3, Image = "Image 2", HotelId = 1 });
                    appContext.Rooms.Add(new Room { RoomId = 3, Name = "Room 3", Capacity = 4, Image = "Image 3", HotelId = 1 });
                    appContext.Rooms.Add(new Room { RoomId = 4, Name = "Room 4", Capacity = 2, Image = "Image 4", HotelId = 2 });
                    appContext.Rooms.Add(new Room { RoomId = 5, Name = "Room 5", Capacity = 3, Image = "Image 5", HotelId = 2 });
                    appContext.Rooms.Add(new Room { RoomId = 6, Name = "Room 6", Capacity = 4, Image = "Image 6", HotelId = 2 });
                    appContext.Rooms.Add(new Room { RoomId = 7, Name = "Room 7", Capacity = 2, Image = "Image 7", HotelId = 3 });
                    appContext.Rooms.Add(new Room { RoomId = 8, Name = "Room 8", Capacity = 3, Image = "Image 8", HotelId = 3 });
                    appContext.Rooms.Add(new Room { RoomId = 9, Name = "Room 9", Capacity = 4, Image = "Image 9", HotelId = 3 });
                    appContext.SaveChanges();
                }
            });
        }).CreateClient();
    }

    [Trait("Category", "Meus testes")]
    [Theory(DisplayName = "Executando meus testes req 2")]
    [InlineData("/city")]
    public async Task TestGet(string url)
    {
        var response = await _clientTest.GetAsync(url);

        Assert.Equal(System.Net.HttpStatusCode.OK, response?.StatusCode);

        var responseBodyString = await response.Content.ReadAsStringAsync();
        var cities = JsonConvert.DeserializeObject<List<CityDto>>(responseBodyString);
        Assert.NotEmpty(cities);
        Assert.Contains("Manaus", cities?[0].Name);
        Assert.Contains("Palmas", cities?[1].Name);
    }

    [Trait("Category", "Meus testes")]
    [Theory(DisplayName = "Executando meus testes na rota post /city req 3")]
    [InlineData("/city")]
    public async Task TestPostCity(string url)
    {
        var newCity = new StringContent(
            JsonConvert.SerializeObject(new CityDto
            {
                Name = "Nova Iguaçu"
            }),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _clientTest.PostAsync(url, newCity);

        var responseString = await response.Content.ReadAsStringAsync();
        CityDto jsonResponse = JsonConvert.DeserializeObject<CityDto>(responseString);

        Assert.Equal(System.Net.HttpStatusCode.Created, response?.StatusCode);
        Assert.Equal(3, jsonResponse?.CityId);
        Assert.Equal("Nova Iguaçu", jsonResponse?.Name);
    }


    [Trait("Category", "Meus testes")]
    [Theory(DisplayName = "Testando endpoint de hotéis req 4")]
    [InlineData("/hotel")]
    public async Task TestGetHotels(string url)
    {
        var response = await _clientTest.GetAsync(url);

        Assert.Equal(System.Net.HttpStatusCode.OK, response?.StatusCode);

        var responseBodyString = await response.Content.ReadAsStringAsync();

        var hotels = JsonConvert.DeserializeObject<List<HotelDto>>(responseBodyString);

        Assert.NotEmpty(hotels);

        var firstHotel = hotels.First();
        Assert.Equal("Trybe Hotel Palmas", firstHotel.Name);
        Assert.Equal("Address 2", firstHotel.Address);
        Assert.Equal("Palmas", firstHotel.CityName);

    }

    [Trait("Category", "Meus testes")]
    [Theory(DisplayName = "Executando meus testes na rota post /hotel req 5")]
    [InlineData("/hotel")]
    public async Task TestPostHotel(string url)
    {
        var newHotel = new StringContent(
            JsonConvert.SerializeObject(new HotelDto
            {
                Name = "Copacabana Palace",
                Address = "Avenida Atlântica, 1500",
                CityId = 2
            }),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _clientTest.PostAsync(url, newHotel);

        var responseString = await response.Content.ReadAsStringAsync();
        HotelDto jsonResponse = JsonConvert.DeserializeObject<HotelDto>(responseString);

        Assert.Equal(System.Net.HttpStatusCode.Created, response?.StatusCode);
        Assert.Equal(2, jsonResponse.CityId);
        Assert.Equal("Palmas", jsonResponse.CityName);
        Assert.Equal("Copacabana Palace", jsonResponse.Name);
        Assert.Equal("Avenida Atlântica, 1500", jsonResponse.Address);
        Assert.Equal(4, jsonResponse.HotelId);
    }

    [Trait("Category", "Meus testes")]
    [Theory(DisplayName = "Testando endpoint de quartos req 6")]
    [InlineData("/room/1")]
    public async Task TestGetRooms(string url)
    {
        var response = await _clientTest.GetAsync(url);

        Assert.Equal(System.Net.HttpStatusCode.OK, response?.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var rooms = JsonConvert.DeserializeObject<List<RoomDto>>(responseContent);


        Assert.NotEmpty(rooms);
        var firstRoom = rooms.First();
        Assert.Equal(3, firstRoom.RoomId);
        Assert.Equal("Room 3", firstRoom.Name);
        Assert.Equal(4, firstRoom.Capacity);
        Assert.Equal("Image 3", firstRoom.Image);
    }

    [Trait("Category", "Meus testes")]
    [Theory(DisplayName = "Executando meus testes na rota post /room req 7")]
    [InlineData("/room")]
    public async Task TestPostRoom(string url)
    {
        var newRoom = new StringContent(
            JsonConvert.SerializeObject(new Room
            {
                Name = "Suite básica",
                Capacity = 2,
                Image = "image suite",
                HotelId = 1
            }),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _clientTest.PostAsync(url, newRoom);

        var responseString = await response.Content.ReadAsStringAsync();
        RoomDto jsonResponse = JsonConvert.DeserializeObject<RoomDto>(responseString);

        Assert.Equal(System.Net.HttpStatusCode.Created, response?.StatusCode);
        Assert.Equal(10, jsonResponse.RoomId);
        Assert.Equal("Suite básica", jsonResponse.Name);
        Assert.Equal(2, jsonResponse.Capacity);
        Assert.Equal("image suite", jsonResponse.Image);
        Assert.Equal(1, jsonResponse.Hotel.HotelId);
        Assert.Equal("Trybe Hotel Manaus", jsonResponse.Hotel.Name);
    }

    [Trait("Category", "Meus testes")]
    [Theory(DisplayName = "Deletar um quarto por um id")]
    [InlineData("/room/1")]
    public async Task TestRoomController(string url)
    {
        var response = await _clientTest.DeleteAsync(url);

        Assert.Equal(System.Net.HttpStatusCode.NoContent, response?.StatusCode);

    }

    [Trait("Category", "Meus testes")]
    [Theory(DisplayName = "Verifica se retorna um erro em rota inexistente")]
    [InlineData("/cities")]
    public async Task TestNotFound(string url)
    {
        var response = await _clientTest.GetAsync(url);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response?.StatusCode);
    }

}