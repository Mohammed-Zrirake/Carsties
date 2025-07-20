using MassTransit;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Models;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPoly());
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint("search-uction-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(5,5));
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });
        cfg.ConfigureEndpoints(context);
    });
});
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
                                                      .WaitAndRetryForeverAsync(_=>TimeSpan.FromSeconds(3));