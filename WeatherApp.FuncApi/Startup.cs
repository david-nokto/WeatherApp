using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using WeatherApp.FuncApi;
using WeatherApp.FuncApi.Services;

[assembly: FunctionsStartup(typeof(Startup))]

namespace WeatherApp.FuncApi
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient<IWeatherDataService, WeatherDataService>(client => client.BaseAddress = new Uri("https://opendata-download-metfcst.smhi.se"));
        }
    }
}
