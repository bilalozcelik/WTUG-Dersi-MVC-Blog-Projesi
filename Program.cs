using BlogProjesi.Data;
using BlogProjesi.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext Ekleme
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication (Oturum / Cookie) Ekleme
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Hesap/Giris";
        options.AccessDeniedPath = "/Hesap/Yetkisiz";
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Veritabanı ve Tabloların Otomatik Oluşturulması İçin (Öğrenme Ortamı Pratikliği)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Veritabanı yoksa oluşturur (varsa dokunmaz, verileriniz silinmez)
    context.Database.EnsureCreated();

    // Varsayılan Admin Kullanıcısını Ekle veya Güncelle (Seeding)
    var adminUser = context.Kullanicilar.FirstOrDefault(u => u.Eposta == "admin@example.com");
    if (adminUser == null)
    {
        context.Kullanicilar.Add(new Kullanici
        {
            AdSoyad = "Sistem Yöneticisi",
            Eposta = "admin@example.com",
            Sifre = "admin",
            Rol = "Admin"
        });
        context.SaveChanges();
    }
    else if (adminUser.AdSoyad == "Sistem Admini")
    {
        adminUser.AdSoyad = "Sistem Yöneticisi";
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
