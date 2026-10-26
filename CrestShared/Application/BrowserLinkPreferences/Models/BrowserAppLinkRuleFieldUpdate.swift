import Foundation

/// The one app-link rule field a settings edit changes; the core applies it.
enum BrowserAppLinkRuleFieldUpdate: Equatable, Sendable {
    case isEnabled(Bool)
    case match(LinkRouteMatch)
    case pattern(String)
    case device(AppLinkDevice)
    case destination(AppLinkDestination)
    case detail(String)

    /// The intent that changes this field of rule `id`.
    func edit(of id: UUID) -> EditAppLinkRule {
        switch self {
        case .isEnabled(let value):
            EditAppLinkRule(
                ruleID: id, isEnabled: value, match: nil, pattern: nil, device: nil, destination: nil, detail: nil)
        case .match(let value):
            EditAppLinkRule(
                ruleID: id, isEnabled: nil, match: value, pattern: nil, device: nil, destination: nil, detail: nil)
        case .pattern(let value):
            EditAppLinkRule(
                ruleID: id, isEnabled: nil, match: nil, pattern: value, device: nil, destination: nil, detail: nil)
        case .device(let value):
            EditAppLinkRule(
                ruleID: id, isEnabled: nil, match: nil, pattern: nil, device: value, destination: nil, detail: nil)
        case .destination(let value):
            EditAppLinkRule(
                ruleID: id, isEnabled: nil, match: nil, pattern: nil, device: nil, destination: value, detail: nil)
        case .detail(let value):
            EditAppLinkRule(
                ruleID: id, isEnabled: nil, match: nil, pattern: nil, device: nil, destination: nil, detail: value)
        }
    }
}
