using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WeatherApp.Client.Services;
using WeatherAppClient;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient<ISensorDataService, SensorDataService>(client => client.BaseAddress = new Uri("http://localhost:7071"));
builder.Services.AddScoped<INotifierService, NotifierService>();

builder.Services
  .AddBlazorise(options =>
  {
      options.Immediate = true;
  })
  .AddBootstrapProviders()
  .AddFontAwesomeIcons();

await builder.Build().RunAsync();
