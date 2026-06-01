using Obligatorio_N3D_342742_360021_Client.Services.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Definine el cliente HTTP para consumir la API
builder.Services.AddHttpClient("Api", c => c.BaseAddress = new Uri("https://localhost:7077/"));
// Definine el cliente HTTP para consumir la Map de Google
// builder.Services.AddHttpClient("GoogleMaps", c => c.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/"));

// Agregar el servicio de Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Registrar auxiliar http (síncrono simple)
builder.Services.AddScoped<AuxiliarClienteHttp>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// Agregar el middleware de Session 
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Login}")
    .WithStaticAssets();


app.Run();