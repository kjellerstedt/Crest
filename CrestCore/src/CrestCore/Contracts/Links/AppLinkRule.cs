namespace CrestCore.Contracts;

/// One ordered rule that sends a link followed from a page to another app: a
/// pattern picks the links, `Device` the devices, and `Destination`
/// the app, with the `Detail` it asks for, such as a shortcut's name. A rule
/// with an empty pattern, or whose destination still lacks its detail, never
/// applies.
public sealed record AppLinkRule(Guid Id, bool IsEnabled, LinkPattern Pattern, AppLinkDevice Device, AppLinkDestination Destination,
    string Detail) {
    #region Actions - Routing

    /// Whether the rule applies to `url` on the device the app runs on,
    /// `running`.
    internal bool Matches(string url, AppLinkDevice running) =>
        Device.Applies(running) && Address(url) is not null && Pattern.Matches(url);

    /// The address the system opens for `url`, or null while the destination
    /// lacks its detail.
    internal string? Address(string url) => Destination.Address(Detail, url);

    #endregion
}
