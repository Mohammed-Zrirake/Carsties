using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Models;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPoly());
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();
app.MapControllers();
await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(builder.Configuration.GetConnectionString("MongoDbConnection")));
await DB.Index<Item>()
    .Key(x => x.Make, KeyType.Text)
    .Key(x => x.Model, KeyType.Text)
    .Key(x => x.Color, KeyType.Text)
    .CreateAsync();

try
{
    await DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine("Error initializing database: " + ex.Message);
}
 
app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPoly() => HttpPolicyExtensions
                                                      .HandleTransientHttpError()
                                                      .OrResult(mes => mes.StatusCode == System.Net.HttpStatusCode.NotFound)
                                                      .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));