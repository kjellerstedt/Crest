using System.Text.RegularExpressions;

namespace CrestCore.Contracts;

/// Where an app-link rule sends a link: an app Crest knows how to address, or
/// a template the person writes. Each destination builds the address the
/// system opens from the rule's detail and the link, so an app that changes
/// its address changes it here, for every saved rule.
///
/// A rule spells a destination as its `Name`, so a name never changes. A
/// destination travels as its index in `All`, so `All` is append-only.
public sealed class AppLinkDestination {
    #region Static Variables

    public const int MaximumDetailLength = 2048;

    private static readonly Regex Placeholder = new(@"\{(url|encodedUrl)\}", RegexOptions.CultureInvariant);

    /// Schemes a template may not open: they run script or read local files
    /// rather than hand a link to an app.
    private static readonly string[] ForbiddenSchemes = ["javascript", "data", "file", "blob", "about"];

    #endregion

    #region Variables

    /// Safari, where its extensions run. `x-safari-` names Safari even when
    /// Crest is the default browser, where a plain address would come straight
    /// back.
    public static readonly AppLinkDestination Safari = new(name: "safari", title: "Safari", detailTitle: "",
        isValidDetail: detail => detail.Length == 0, address: (_, url) => "x-safari-" + url);

    /// Narwhal 2, from the address its community userscripts use.
    public static readonly AppLinkDestination Narwhal = new(name: "narwhal", title: "Narwhal", detailTitle: "",
        isValidDetail: detail => detail.Length == 0, address: (_, url) => "narwhal://open-url/" + Uri.EscapeDataString(url));

    /// A shortcut run with the link as its text input; the detail names it.
    public static readonly AppLinkDestination Shortcut = new(name: "shortcut", title: "Shortcut", detailTitle: "Shortcut Name",
        isValidDetail: detail => detail.Length <= MaximumDetailLength,
        address: (name, url) => name.Length == 0 ? null
            : "shortcuts://run-shortcut?name=" + Uri.EscapeDataString(name) + "&input=text&text=" + Uri.EscapeDataString(url));

    /// The address the person writes as the detail, in which `{url}` stands for
    /// the link as it is and `{encodedUrl}` for the link percent-encoded to sit
    /// inside another address.
    public static readonly AppLinkDestination Custom = new(name: "custom", title: "Custom", detailTitle: "Address",
        isValidDetail: IsValidTemplate, address: (template, url) => template.Length == 0 ? null : Expanded(template, url));

    public static IReadOnlyList<AppLinkDestination> All { get; } = [Safari, Narwhal, Shortcut, Custom];

    public string Name { get; }

    /// What the rule editor calls the destination.
    [Localized]
    public string Title { get; }

    /// What the rule editor calls the detail the destination asks for; empty
    /// when it asks for none.
    [Localized]
    public string DetailTitle { get; }

    /// The rule editor asks for a detail.
    public bool AsksForDetail => DetailTitle.Length > 0;

    private readonly Func<string, bool> isValidDetail;
    private readonly Func<string, string, string?> address;

    #endregion

    #region Constructors

    private AppLinkDestination(string name, string title, string detailTitle, Func<string, bool> isValidDetail,
        Func<string, string, string?> address) {
        Name = name;
        Title = title;
        DetailTitle = detailTitle;
        this.isValidDetail = isValidDetail;
        this.address = address;
    }

    #endregion

    #region Actions - Lookup

    public static AppLinkDestination? Named(string? name) => All.FirstOrDefault(destination => destination.Name == name);

    #endregion

    #region Actions - Addressing

    /// Whether `detail` is one the destination accepts: none for an app Crest
    /// addresses itself, and for a template, empty to leave it unset or one
    /// that holds a placeholder and, with a link in it, reads as an address
    /// with a scheme that hands the link to an app.
    public bool Accepts(string detail) {
        ArgumentNullException.ThrowIfNull(detail);
        return detail.Length <= MaximumDetailLength && isValidDetail(detail);
    }

    /// The address the system opens for `url`, or null while the destination
    /// still lacks the detail it needs.
    public string? Address(string detail, string url) {
        ArgumentNullException.ThrowIfNull(detail);
        ArgumentNullException.ThrowIfNull(url);
        return address(detail, url);
    }

    private static bool IsValidTemplate(string template) {
        if (template.Length == 0) return true;
        if (!Placeholder.IsMatch(template)) return false;
        return Uri.TryCreate(Expanded(template, "https://example.com/"), UriKind.Absolute, out var parsed)
            && !ForbiddenSchemes.Contains(parsed.Scheme, StringComparer.OrdinalIgnoreCase);
    }

    /// The template with each placeholder replaced in one pass, so a link that
    /// spells a placeholder is never replaced twice.
    private static string Expanded(string template, string url) => Placeholder.Replace(template,
        placeholder => placeholder.Groups[1].Value == "url" ? url : Uri.EscapeDataString(url));

    #endregion
}
