using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace WeatherAppClient.Shared
{
    public partial class NavBar
    {
        [Inject]
        NavigationManager NavigationManager { get; set; } = default!;

        protected override void OnInitialized() => NavigationManager.LocationChanged += (s, e) => StateHasChanged();

        bool IsActive(string href, NavLinkMatch navLinkMatch = NavLinkMatch.Prefix)
        {
            var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLower();
            return navLinkMatch == NavLinkMatch.All ? relativePath == href.ToLower() : relativePath.StartsWith(href.ToLower());
        }

        string GetActive(string href, NavLinkMatch navLinkMatch = NavLinkMatch.Prefix) => IsActive(href, navLinkMatch) ? "active" : "";
    }
}