
var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages services
builder.Services.AddRazorPages();

var app = builder.Build();

// Handle unexpected server errors
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Handle HTTP errors such as 404
app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();
