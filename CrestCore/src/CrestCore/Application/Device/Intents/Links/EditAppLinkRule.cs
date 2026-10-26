using CrestCore.Application;
using CrestCore.Domain;

namespace CrestCore.Contracts;

/// Changes exactly one field of an app-link rule, so an edit never overwrites
/// another field with a stale copy; choosing another destination also clears
/// the detail, which belonged to the old one. Refused with `UnknownAppLinkRule`,
/// with `InvalidAppLinkRuleEdit` unless exactly one field is given, with
/// `LinkPatternTooLong` past `LinkPattern.MaximumLength`, and with
/// `InvalidAppLinkDetail` for a detail the rule's destination does not accept.
public sealed record EditAppLinkRule(Guid RuleId, bool? IsEnabled, LinkRouteMatch? Match, string? Pattern, AppLinkDevice? Device,
    AppLinkDestination? Destination, string? Detail) : LinkIntent {
    #region Actions - Device

    internal override void Apply(Device device, DeviceTurn turn) =>
        device.ReviseLinks(turn.Changes, links => links.Editing(this));

    #endregion
}
