using Application.Services;
using Domain.Interfaces;
using Infrastructure.Implementations;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Http Client Definitions
builder.Services.AddHttpClient<Api1Provider>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Providers:Api1:BaseUrl"] ?? "");
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHttpClient<Api2Provider>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHttpClient<Api3Provider>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Providers:Api3:BaseUrl"] ?? "");
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddTransient<IExchangeProvider, Api1Provider>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var client = httpClientFactory.CreateClient(nameof(Api1Provider));

    var apiKey = builder.Configuration["ApiKeys:Api1"] ?? throw new Exception("ApiKey missing");

    return new Api1Provider(client, apiKey);
});

builder.Services.AddTransient<IExchangeProvider, Api2Provider>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var client = httpClientFactory.CreateClient(nameof(Api2Provider));

    return new Api2Provider(client);
});

builder.Services.AddTransient<IExchangeProvider, Api3Provider>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var client = httpClientFactory.CreateClient(nameof(Api3Provider));

    return new Api3Provider(client);
});

//Register service
builder.Services.AddSingleton<ICompareRatesService, CompareRatesService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
