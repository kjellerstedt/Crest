namespace CrestCore.Contracts;

#region Types

/// A Space for the link, and whether it opens as a Quick Window there.
/// <paramref name="SubstitutesForLockedSpace"/> is true when the routed Space was
/// locked and this Space stands in for it.
internal readonly record struct LinkRoutingDecision(bool OpensQuickWindow, Guid SpaceId, bool SubstitutesForLockedSpace = false);

#endregion

/// How links behave on this device: where a link from another app opens, what
/// a clicked link does, how long a Quick Window lives, and the routes that send
/// matching links to a Space, and the rules that send links followed from a page
/// to another app. The device store keeps them and never syncs
/// them. Each edit answers the revised preferences, or throws the rule that
/// refuses it.
public sealed record LinkPreferences(ExternalLinkDestination Destination, Guid? DestinationSpaceId, bool FocusesNewTabs,
    bool FollowsMovedTabs, bool OpensPeekAutomatically, LinkPeekModifier PeekModifier, bool DragsLinksToPeek,
    QuickWindowArchivePolicy ArchivePolicy, bool RemembersSpaceBySite, IReadOnlyList<LinkRoute> Routes,
    IReadOnlyList<RememberedSite> RememberedSites, IReadOnlyList<AppLinkRule> AppRules) {
    #region Static Variables

    /// The preferences of a device that never chose any.
    internal static readonly LinkPreferences Default = new(ExternalLinkDestination.QuickWindow, DestinationSpaceId: null,
        FocusesNewTabs: false, FollowsMovedTabs: true, OpensPeekAutomatically: true, LinkPeekModifier.Option,
        DragsLinksToPeek: true, QuickWindowArchivePolicy.After6Hours, RemembersSpaceBySite: true, [], [], []);

    #endregion

    #region Actions - Route edits

    /// A new route starts enabled, matching by containment, with an empty
    /// pattern the person fills in; an empty pattern never matches.
    internal LinkPreferences Adding(AddLinkRoute intent) {
        ArgumentNullException.ThrowIfNull(intent);
        if (Routes.Count >= AddLinkRoute.MaximumRoutes) throw new Rejected(new LinkRoutesFull(AddLinkRoute.MaximumRoutes));
        if (intent.RouteId == Guid.Empty || Routes.Any(route => route.Id == intent.RouteId))
            throw new Rejected(new LinkRouteExists(intent.RouteId));
        if (intent.DestinationSpaceId == Guid.Empty) throw new Rejected(new InvalidLinkRouteEdit(intent.RouteId));
        return this with {
            Routes = [.. Routes, new LinkRoute(intent.RouteId, true, LinkPattern.Unset, intent.DestinationSpaceId)]
        };
    }

    /// Changes exactly one field, so concurrent edits to other fields of the
    /// same route are never overwritten by a stale copy.
    internal LinkPreferences Editing(EditLinkRoute edit) {
        ArgumentNullException.ThrowIfNull(edit);
        var route = ExistingRoute(edit.RouteId);
        int supplied = (edit.IsEnabled is null ? 0 : 1) + (edit.Match is null ? 0 : 1) + (edit.Pattern is null ? 0 : 1)
            + (edit.DestinationSpaceId is null ? 0 : 1);
        if (supplied != 1 || edit.DestinationSpaceId == Guid.Empty) throw new Rejected(new InvalidLinkRouteEdit(edit.RouteId));
        if (edit.Pattern is { Length: > LinkPattern.MaximumLength })
            throw new Rejected(new LinkPatternTooLong(LinkPattern.MaximumLength));
        var edited = route with {
            IsEnabled = edit.IsEnabled ?? route.IsEnabled,
            Pattern = route.Pattern.Edited(edit.Match, edit.Pattern),
            DestinationSpaceId = edit.DestinationSpaceId ?? route.DestinationSpaceId
        };
        return this with { Routes = [.. Routes.Select(candidate => candidate.Id == edit.RouteId ? edited : candidate)] };
    }

    /// Moves one route by an offset; a move past either end changes nothing.
    internal LinkPreferences Moving(MoveLinkRoute move) {
        ArgumentNullException.ThrowIfNull(move);
        return Moved(Routes, ExistingRoute(move.RouteId), move.Offset) is { } routes ? this with { Routes = routes } : this;
    }

    internal LinkPreferences Removing(RemoveLinkRoute removal) {
        ArgumentNullException.ThrowIfNull(removal);
        var route = ExistingRoute(removal.RouteId);
        return this with { Routes = [.. Routes.Where(candidate => candidate != route)] };
    }

    #endregion

    #region Actions - App link rule edits

    /// A new rule starts enabled, matching by containment, for both devices,
    /// opening in Safari, with an empty pattern the person fills in; such a
    /// rule never applies.
    internal LinkPreferences Adding(AddAppLinkRule intent) {
        ArgumentNullException.ThrowIfNull(intent);
        if (AppRules.Count >= AddAppLinkRule.MaximumRules) throw new Rejected(new AppLinkRulesFull(AddAppLinkRule.MaximumRules));
        if (intent.RuleId == Guid.Empty || AppRules.Any(rule => rule.Id == intent.RuleId))
            throw new Rejected(new AppLinkRuleExists(intent.RuleId));
        return this with {
            AppRules = [.. AppRules, new AppLinkRule(intent.RuleId, true, LinkPattern.Unset, AppLinkDevice.Any, AppLinkDestination.Safari, "")]
        };
    }

    /// Changes exactly one field, so concurrent edits to other fields of the
    /// same rule are never overwritten by a stale copy.
    internal LinkPreferences Editing(EditAppLinkRule edit) {
        ArgumentNullException.ThrowIfNull(edit);
        var rule = ExistingAppRule(edit.RuleId);
        int supplied = (edit.IsEnabled is null ? 0 : 1) + (edit.Match is null ? 0 : 1) + (edit.Pattern is null ? 0 : 1)
            + (edit.Device is null ? 0 : 1) + (edit.Destination is null ? 0 : 1) + (edit.Detail is null ? 0 : 1);
        if (supplied != 1) throw new Rejected(new InvalidAppLinkRuleEdit(edit.RuleId));
        if (edit.Pattern is { Length: > LinkPattern.MaximumLength })
            throw new Rejected(new LinkPatternTooLong(LinkPattern.MaximumLength));
        if (edit.Detail is { } detail && !rule.Destination.Accepts(detail)) throw new Rejected(new InvalidAppLinkDetail());
        var edited = rule with {
            IsEnabled = edit.IsEnabled ?? rule.IsEnabled,
            Pattern = rule.Pattern.Edited(edit.Match, edit.Pattern),
            Device = edit.Device ?? rule.Device,
            Destination = edit.Destination ?? rule.Destination,
            Detail = edit.Destination is null ? edit.Detail ?? rule.Detail : ""
        };
        return this with { AppRules = [.. AppRules.Select(candidate => candidate.Id == edit.RuleId ? edited : candidate)] };
    }

    /// Moves one rule by an offset; a move past either end changes nothing.
    internal LinkPreferences Moving(MoveAppLinkRule move) {
        ArgumentNullException.ThrowIfNull(move);
        return Moved(AppRules, ExistingAppRule(move.RuleId), move.Offset) is { } rules ? this with { AppRules = rules } : this;
    }

    internal LinkPreferences Removing(RemoveAppLinkRule removal) {
        ArgumentNullException.ThrowIfNull(removal);
        var rule = ExistingAppRule(removal.RuleId);
        return this with { AppRules = [.. AppRules.Where(candidate => candidate != rule)] };
    }

    #endregion

    #region Actions - Sites

    /// Remembers `spaceId` for `url`'s site while the preferences remember
    /// Spaces by site and the address has a host; otherwise changes nothing.
    internal LinkPreferences Remembering(string url, Guid spaceId) {
        if (!RemembersSpaceBySite || RememberedSite.SiteOf(url) is not { } site) return this;
        return this with {
            RememberedSites = [.. RememberedSites.Where(remembered => remembered.Site != site), new RememberedSite(site, spaceId)]
        };
    }

    #endregion

    #region Actions - Space deletion

    /// A deleted Space takes its routes with it, stops being the chosen
    /// external-link Space, and is forgotten for every site that remembered it.
    /// Routing already skips missing Spaces; this keeps the stored preferences
    /// from pointing at a Space that no longer exists.
    internal LinkPreferences Forgetting(Guid spaceId) => this with {
        Routes = [.. Routes.Where(route => route.DestinationSpaceId != spaceId)],
        DestinationSpaceId = DestinationSpaceId == spaceId ? null : DestinationSpaceId,
        RememberedSites = [.. RememberedSites.Where(remembered => remembered.SpaceId != spaceId)]
    };

    #endregion

    #region Actions - Routing

    /// Where a link from another process goes under these preferences when
    /// some Spaces are locked. A locked Space is never unlocked on such a
    /// link's behalf, whether a route, the chosen Space or a remembered site
    /// names it: the link opens in a Quick Window on the selected Space when
    /// that one is unlocked and available, else on the first that is. Null
    /// when no Space can take it.
    internal LinkRoutingDecision? RouteExternal(string url, LinkRoutingContext context, IReadOnlyCollection<Guid> locked) {
        ArgumentNullException.ThrowIfNull(locked);
        var routed = Route(url, context);
        if (!locked.Contains(routed.SpaceId)) return routed;
        bool Open(Guid space) => context.IsAvailable(space) && !locked.Contains(space);
        Guid? substitute = Open(context.SelectedSpaceId) ? context.SelectedSpaceId
            : context.Spaces.Where(Open).Select(space => (Guid?)space).FirstOrDefault();
        return substitute is { } space ? new(true, space, SubstitutesForLockedSpace: true) : null;
    }

    /// Where a link opened from outside Crest goes: the first enabled route
    /// that matches and whose Space can open, else the external-link
    /// destination.
    internal LinkRoutingDecision Route(string url, LinkRoutingContext context) {
        ArgumentNullException.ThrowIfNull(context);
        var route = Routes.FirstOrDefault(candidate => candidate.IsEnabled
            && context.IsAvailable(candidate.DestinationSpaceId) && candidate.Pattern.Matches(url));
        if (route is not null) return new(false, route.DestinationSpaceId);
        var remembered = RememberedSite.SiteOf(url) is { } site ? RememberedSites.FirstOrDefault(entry => entry.Site == site)?.SpaceId : null;
        var destination = Destination;
        return new(destination.OpensQuickWindow, destination.Space(this, remembered, context));
    }

    #endregion

    #region Actions - App routing

    /// The address the system opens for a link followed from a page, from the
    /// first enabled rule that applies on the device the app runs on,
    /// `running`; null when none does and the page loads the link.
    internal string? RouteToApp(string url, AppLinkDevice running) {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(running);
        return AppRules.FirstOrDefault(rule => rule.IsEnabled && rule.Matches(url, running))?.Address(url);
    }

    #endregion

    #region Actions - Equality

    public bool Equals(LinkPreferences? other) => other is not null
        && Destination == other.Destination && DestinationSpaceId == other.DestinationSpaceId
        && FocusesNewTabs == other.FocusesNewTabs && FollowsMovedTabs == other.FollowsMovedTabs
        && OpensPeekAutomatically == other.OpensPeekAutomatically && PeekModifier == other.PeekModifier
        && DragsLinksToPeek == other.DragsLinksToPeek && ArchivePolicy == other.ArchivePolicy
        && RemembersSpaceBySite == other.RemembersSpaceBySite && Routes.SequenceEqual(other.Routes)
        && RememberedSites.SequenceEqual(other.RememberedSites) && AppRules.SequenceEqual(other.AppRules);

    public override int GetHashCode() => HashCode.Combine(Destination, DestinationSpaceId, Routes.Count, RememberedSites.Count, AppRules.Count);

    #endregion

    #region Mutators

    /// `items` with `item` moved `offset` places; null when that passes either
    /// end.
    private static List<T>? Moved<T>(IReadOnlyList<T> items, T item, int offset) {
        var moved = items.ToList();
        int source = moved.IndexOf(item);
        long destination = (long)source + offset;
        if (destination < 0 || destination >= moved.Count) return null;
        moved.RemoveAt(source);
        moved.Insert((int)destination, item);
        return moved;
    }

    private AppLinkRule ExistingAppRule(Guid ruleId) =>
        AppRules.FirstOrDefault(rule => rule.Id == ruleId) ?? throw new Rejected(new UnknownAppLinkRule(ruleId));

    private LinkRoute ExistingRoute(Guid routeId) =>
        Routes.FirstOrDefault(route => route.Id == routeId) ?? throw new Rejected(new UnknownLinkRoute(routeId));

    #endregion
}
