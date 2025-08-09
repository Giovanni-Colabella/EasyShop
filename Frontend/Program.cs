using Frontend.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages().AddSessionStateTempDataProvider();;

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("http://localhost:5150/");

    // Devo usare questo url per docker
    //client.BaseAddress = new Uri("http://api:8080/");
}).AddHttpMessageHandler<JwtCookieHandler>();

builder.Services.AddSession();
builder.Services.AddTransient<JwtCookieHandler>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
