using CrestCore.Application;
using CrestCore.Domain;

namespace CrestCore.Contracts;

/// Adds an app-link rule, `RuleId`, after the others: enabled, matching by
/// containment, for both iPhone and iPad, opening in Safari, with an empty
/// pattern the person fills in. Refused with `AppLinkRulesFull` past `MaximumRules`,
/// and with `AppLinkRuleExists` for an identity already taken.
public sealed record AddAppLinkRule(Guid RuleId) : LinkIntent {
    #region Static Variables

    public const int MaximumRules = 64;

    #endregion

    #region Actions - Device

    internal override void Apply(Device device, DeviceTurn turn) =>
        device.ReviseLinks(turn.Changes, links => links.Adding(this));

    #endregion
}
