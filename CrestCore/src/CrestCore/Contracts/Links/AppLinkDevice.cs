namespace CrestCore.Contracts;

/// The devices an app-link rule applies to, and the device the app runs on. A
/// rule spells it as its `Name`, so a name never changes; it travels as its
/// index in `All`, so `All` is append-only.
public sealed class AppLinkDevice {
    #region Variables

    /// The rule applies on an iPhone and an iPad.
    public static readonly AppLinkDevice Any = new(name: "any", title: "iPhone and iPad", appliesEverywhere: true);

    public static readonly AppLinkDevice IPhone = new(name: "iphone", title: "iPhone", appliesEverywhere: false);

    public static readonly AppLinkDevice IPad = new(name: "ipad", title: "iPad", appliesEverywhere: false);

    public static IReadOnlyList<AppLinkDevice> All { get; } = [Any, IPhone, IPad];

    public string Name { get; }

    /// What the rule editor calls the devices.
    [Localized]
    public string Title { get; }

    private readonly bool appliesEverywhere;

    #endregion

    #region Constructors

    private AppLinkDevice(string name, string title, bool appliesEverywhere) {
        Name = name;
        Title = title;
        this.appliesEverywhere = appliesEverywhere;
    }

    #endregion

    #region Actions - Lookup

    public static AppLinkDevice? Named(string? name) => All.FirstOrDefault(device => device.Name == name);

    #endregion

    #region Actions - Matching

    /// Whether a rule for these devices applies on the device the app runs on,
    /// `running`, which is `IPhone` or `IPad`.
    public bool Applies(AppLinkDevice running) {
        ArgumentNullException.ThrowIfNull(running);
        return appliesEverywhere || this == running;
    }

    #endregion
}
