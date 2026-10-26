using CrestCore.Application;

namespace CrestCore.Contracts;

/// Where a link followed from a page goes, when this device's app-link rules
/// send it to another app: the address the system opens, from the first
/// enabled rule that matches on the device the app runs on, `Device`: `IPhone`
/// or `IPad`. Null when no rule matches, and the page loads the link.
public sealed record RouteAppLink(string Url, AppLinkDevice Device) : Query<AppLinkPlacement> {
    #region Actions - Answering

    internal override AppLinkPlacement Answer(CrestApp app) {
        LinkPreferences preferences;
        lock (app.Device.Gate) preferences = app.Device.Links;
        return new(preferences.RouteToApp(Url, Device));
    }

    #endregion
}
