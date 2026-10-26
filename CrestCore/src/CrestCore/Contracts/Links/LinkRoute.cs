namespace CrestCore.Contracts;

/// One ordered rule that sends matching external links to a Space.
public sealed record LinkRoute(Guid Id, bool IsEnabled, LinkPattern Pattern, Guid DestinationSpaceId);
