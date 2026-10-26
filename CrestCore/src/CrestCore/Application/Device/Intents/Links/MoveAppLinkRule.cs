using CrestCore.Application;
using CrestCore.Domain;

namespace CrestCore.Contracts;

/// Moves an app-link rule `Offset` places among the rules, which match in
/// order. A move past either end changes nothing. Refused with
/// `UnknownAppLinkRule`.
public sealed record MoveAppLinkRule(Guid RuleId, int Offset) : LinkIntent {
    #region Actions - Device

    internal override void Apply(Device device, DeviceTurn turn) =>
        device.ReviseLinks(turn.Changes, links => links.Moving(this));

    #endregion
}
