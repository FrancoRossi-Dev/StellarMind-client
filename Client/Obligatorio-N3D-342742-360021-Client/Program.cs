using Libreria.Infraestuctura.AccesoDatos.EF;
using StellarMinds.AplicationLogic.UseCases.Equipments;
using StellarMinds.AplicationLogic.UseCases.Users;
using StellarMinds.BusinessLogic.Entities.Equipment;
using StellarMinds.Infraestructure.EntityFramework;
using StellarMinds.Infraestructure.Repositories;
using StellarMinds.Shared.DTO.Users;
using StellarMinds.Shared.IRepositories;
using StellarMinds.Shared.IUseCases.Equipments;
using StellarMinds.Shared.IUseCases.Users;
using StellarMinds.Shared.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Agregar soporte de memoria caché
builder.Services.AddDistributedMemoryCache();

// Agregar el servicio de Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<ICelestialObjectsRepository, CelestialObjectsRepository>();

// UseCases - Users
builder.Services.AddScoped<ICURegisterUser<RegisterUserDTO>, RegisterUser>();
builder.Services.AddScoped<ICULoginUser<LogInUserVM>, LoginUser>();
builder.Services.AddScoped<ICUListAllUsers<UserVM>, ListUsers>();
builder.Services.AddScoped<ICUDeleteUser, DeleteUser>();

// UseCases - Equipments
builder.Services.AddScoped<IUCRegisterEquipment<Equipment>, RegisterEquipments>();
builder.Services.AddScoped<IUCGetEquipments<Equipment>, GetEquipments>();

// Stellar context
builder.Services.AddDbContext<StellarContext>();

// seed
builder.Services.AddScoped<SeedData>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsProduction())
{
    var seeder = app.Services.CreateScope().ServiceProvider.GetRequiredService<SeedData>();
    seeder.Run();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();

// Agregar el middleware de Session 
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=user}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
