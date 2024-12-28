using Microsoft.EntityFrameworkCore;
using WEBBERBERODEV.DATA;
using Microsoft.AspNetCore.Identity;
using WEBBERBERODEV.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using WEBBERBERODEV.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// DbContext ayarlar�
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity ayarlar�: IdentityUser yerine ApplicationUser kullan�l�yor
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddRazorPages();

// E-posta g�nderme hizmeti
builder.Services.AddScoped<IEmailSender, EmailSender>();

// Di�er servisler
builder.Services.AddScoped<EmployeeDailyEarningsService>();



// HttpClient Factory'yi ekleme
builder.Services.AddHttpClient();


// Hairstyle Changer Hizmeti
builder.Services.AddScoped<HairstyleChangerService>(serviceProvider =>
{
    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    return new HairstyleChangerService(httpClient, configuration);
});




// Swagger Ayarlar� (Basitle�tirilmi�)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "WEBBERBERODEV API", Version = "v1" });
});

var app = builder.Build();

// Middleware Konfig�rasyonlar�

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WEBBERBERODEV API V1");
        c.RoutePrefix = "swagger"; // Swagger UI'ye eri�mek i�in /swagger yolunu kullanabilirsiniz
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication ve Authorization middleware'leri
app.UseAuthentication();
app.UseAuthorization();

// Swagger Middleware'i �retim Ortam�nda Kullanmak �stiyorsan�z, a�a��daki sat�r� kald�r�n.
// �u anda sadece geli�tirme ortam�nda �al���yor.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WEBBERBERODEV API V1");
        c.RoutePrefix = "swagger";
    });
}

app.MapRazorPages();
app.MapControllers(); // API controller'lar i�in
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
