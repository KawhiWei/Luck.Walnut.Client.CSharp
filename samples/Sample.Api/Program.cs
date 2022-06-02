var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers(
    // x=>
//     x.Filters.Add<aaa>();//这种是scope
// x.Filters.Add(new aaa());//这种是单例
);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLuckWalnutConfig(builder.Configuration, builder.Configuration, x =>
{
    x.ServerUri = "http://localhost:5099";
    x.WebSocketUri = "http://localhost:5099/im";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseAuthorization();

app.MapControllers();

app.Run();


