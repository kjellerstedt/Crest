import SwiftUI

/// The rules that hand a link tapped in a page to another app, such as a
/// Reddit link to Narwhal. The first matching rule wins.
struct BrowserAppLinkRulesSection: View {
    let rules: [AppLinkRule]
    let update: (UUID, BrowserAppLinkRuleFieldUpdate) -> Bool
    let remove: (UUID) -> Void
    let move: (UUID, Int) -> Void
    let add: () -> Void

    var body: some View {
        Section {
            if rules.isEmpty {
                Text("No rules yet.")
                    .foregroundStyle(.secondary)
            } else {
                ForEach(Array(rules.enumerated()), id: \.element.id) { index, rule in
                    BrowserAppLinkRuleEditor(
                        rule: rule,
                        canMoveUp: index > 0,
                        canMoveDown: index + 1 < rules.count,
                        update: { field in update(rule.id, field) },
                        delete: { remove(rule.id) },
                        moveUp: { move(rule.id, -1) },
                        moveDown: { move(rule.id, 1) }
                    )
                }
            }

            Button("New Rule", systemImage: "plus", action: add)
                .buttonStyle(.bordered)
                .accessibilityIdentifier("app-link-rule-add")
        } header: {
            Text("Open in Apps")
        } footer: {
            CrestFormFootnote("Links you tap in a page open in another app. The first matching rule wins.")
        }
    }
}
