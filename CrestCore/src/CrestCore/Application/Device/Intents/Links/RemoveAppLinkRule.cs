using CrestCore.Application;
using CrestCore.Domain;

namespace CrestCore.Contracts;

/// Removes an app-link rule. Refused with `UnknownAppLinkRule`.
public sealed record RemoveAppLinkRule(Guid RuleId) : LinkIntent {
    #region Actions - Device

    internal override void Apply(Device device, DeviceTurn turn) =>
        device.ReviseLinks(turn.Changes, links => links.Removing(this));

    #endregion
}
