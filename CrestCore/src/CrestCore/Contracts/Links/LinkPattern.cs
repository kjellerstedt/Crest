using CrestCore.Domain;

namespace CrestCore.Contracts;

/// What picks the links a rule applies to: `Text`, compared with a link's
/// normalized address the way `Match` says. Routes and app-link rules each
/// hold one. Text that is empty once trimmed matches nothing.
public sealed record LinkPattern(LinkRouteMatch Match, string Text) {
    #region Static Variables

    public const int MaximumLength = 2048;

    /// What a new rule starts with: containment, with no text yet.
    internal static readonly LinkPattern Unset = new(LinkRouteMatch.Contains, "");

    #endregion

    #region Actions - Matching

    /// Whether the pattern matches `url`.
    internal bool Matches(string url) {
        string text = Text.Trim();
        if (text.Length == 0) return false;
        return Match.Matches(Normalized(url), text, Normalized);
    }

    private static string Normalized(string address) => new WebAddress(address).Normalized ?? address;

    #endregion

    #region Actions - Editing

    /// The pattern with the given parts changed, and the rest kept.
    internal LinkPattern Edited(LinkRouteMatch? match, string? text) => new(match ?? Match, text ?? Text);

    #endregion
}
