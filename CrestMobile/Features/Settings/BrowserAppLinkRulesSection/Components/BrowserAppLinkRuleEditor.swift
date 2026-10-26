import SwiftUI

struct BrowserAppLinkRuleEditor: View {
    let rule: AppLinkRule
    let canMoveUp: Bool
    let canMoveDown: Bool
    let update: (BrowserAppLinkRuleFieldUpdate) -> Bool
    let delete: () -> Void
    let moveUp: () -> Void
    let moveDown: () -> Void

    @FocusState private var focusedField: Field?
    @State private var presentsMatchChoices = false
    @State private var presentsDeviceChoices = false
    @State private var presentsDestinationChoices = false
    @State private var detailDraft = ""
    @State private var detailIsRefused = false

    private enum Field {
        case pattern
        case detail
    }

    var body: some View {
        VStack(alignment: .leading, spacing: BrowserPlatformLinkRouteLayout.contentSpacing) {
            Toggle("Enabled", isOn: binding(rule.isEnabled, field: BrowserAppLinkRuleFieldUpdate.isEnabled))
                .frame(minHeight: BrowserSettingsControlMetrics.minimumTouchTarget)
                .accessibilityIdentifier("app-link-rule-enabled-\(identifierSuffix)")

            Button {
                presentsMatchChoices = true
            } label: {
                choiceLabel(title: "Match", value: String(localized: rule.pattern.match.title))
            }
            .buttonStyle(.plain)
            .accessibilityValue(Text(rule.pattern.match.title))
            .accessibilityIdentifier("app-link-rule-match-\(identifierSuffix)")
            .confirmationDialog("Match", isPresented: $presentsMatchChoices, titleVisibility: .visible) {
                ForEach(LinkRouteMatch.all, id: \.self) { match in
                    Button(match.title) { apply(.match(match)) }
                }
                Button("Cancel", role: .cancel) {}
            }

            TextField("URL or text", text: binding(rule.pattern.text, field: BrowserAppLinkRuleFieldUpdate.pattern))
                .textInputAutocapitalization(.never)
                .autocorrectionDisabled()
                .keyboardType(.URL)
                .submitLabel(.next)
                .focused($focusedField, equals: .pattern)
                .onSubmit { focusedField = .detail }
                .frame(minHeight: BrowserSettingsControlMetrics.minimumTouchTarget)
                .accessibilityIdentifier("app-link-rule-pattern-\(identifierSuffix)")

            Button {
                presentsDeviceChoices = true
            } label: {
                choiceLabel(title: "On", value: String(localized: rule.device.title))
            }
            .buttonStyle(.plain)
            .accessibilityValue(Text(rule.device.title))
            .accessibilityIdentifier("app-link-rule-device-\(identifierSuffix)")
            .confirmationDialog("On", isPresented: $presentsDeviceChoices, titleVisibility: .visible) {
                ForEach(AppLinkDevice.all, id: \.self) { device in
                    Button(device.title) { apply(.device(device)) }
                }
                Button("Cancel", role: .cancel) {}
            }

            Button {
                presentsDestinationChoices = true
            } label: {
                choiceLabel(title: "Open in", value: String(localized: rule.destination.title))
            }
            .buttonStyle(.plain)
            .accessibilityValue(Text(rule.destination.title))
            .accessibilityIdentifier("app-link-rule-destination-\(identifierSuffix)")
            .confirmationDialog("Open in", isPresented: $presentsDestinationChoices, titleVisibility: .visible) {
                ForEach(AppLinkDestination.all, id: \.self) { destination in
                    Button(destination.title) { apply(.destination(destination)) }
                }
                Button("Cancel", role: .cancel) {}
            }

            if rule.destination.asksForDetail {
                detailField
            }

            HStack(spacing: CrestSpacing.small) {
                Button("Move Up", systemImage: "chevron.up", action: moveUp)
                    .disabled(!canMoveUp)
                    .accessibilityIdentifier("app-link-rule-move-up-\(identifierSuffix)")
                Button("Move Down", systemImage: "chevron.down", action: moveDown)
                    .disabled(!canMoveDown)
                    .accessibilityIdentifier("app-link-rule-move-down-\(identifierSuffix)")
                Spacer()
                Button("Delete", systemImage: "trash", role: .destructive, action: delete)
                    .buttonStyle(BrowserSettingsIconButtonStyle(tint: .red))
                    .accessibilityIdentifier("app-link-rule-delete-\(identifierSuffix)")
            }
            .labelStyle(.iconOnly)
            .buttonStyle(BrowserSettingsIconButtonStyle())
        }
        .accessibilityElement(children: .contain)
        .accessibilityIdentifier("app-link-rule-\(identifierSuffix)")
    }

    /// A custom address is only valid once it holds a placeholder, so typing
    /// is kept here and sent when the person finishes.
    private var detailField: some View {
        VStack(alignment: .leading, spacing: CrestSpacing.small) {
            TextField(String(localized: rule.destination.detailTitle), text: $detailDraft)
                .textInputAutocapitalization(.never)
                .autocorrectionDisabled()
                .keyboardType(.URL)
                .submitLabel(.done)
                .focused($focusedField, equals: .detail)
                .onSubmit { focusedField = nil }
                .onChange(of: detailDraft) { detailIsRefused = false }
                .onChange(of: focusedField) { previous, _ in
                    if previous == .detail { commitDetail() }
                }
                .onChange(of: rule.detail, initial: true) { _, detail in
                    if focusedField != .detail { detailDraft = detail }
                }
                .frame(minHeight: BrowserSettingsControlMetrics.minimumTouchTarget)
                .accessibilityIdentifier("app-link-rule-detail-\(identifierSuffix)")

            if rule.destination == .custom {
                Text("Use {url} for the link, or {encodedUrl} to place it inside another address.")
                    .font(.footnote)
                    .foregroundStyle(detailIsRefused ? .red : .secondary)
            }
        }
    }

    private var identifierSuffix: String {
        rule.id.uuidString.lowercased()
    }

    private func choiceLabel(title: LocalizedStringKey, value: String) -> some View {
        HStack(spacing: CrestSpacing.small) {
            Text(title)
                .foregroundStyle(.primary)
            Spacer(minLength: CrestSpacing.medium)
            Text(value)
                .foregroundStyle(.secondary)
                .lineLimit(1)
            Image(systemName: "chevron.up.chevron.down")
                .font(.caption.weight(.semibold))
                .foregroundStyle(.tertiary)
                .accessibilityHidden(true)
        }
        .frame(maxWidth: .infinity, minHeight: BrowserSettingsControlMetrics.minimumTouchTarget)
        .contentShape(.rect)
    }

    private func commitDetail() {
        guard detailDraft != rule.detail else { return }
        detailIsRefused = !update(.detail(detailDraft))
    }

    private func apply(_ field: BrowserAppLinkRuleFieldUpdate) {
        _ = update(field)
    }

    private func binding<Value>(
        _ value: Value,
        field: @escaping (Value) -> BrowserAppLinkRuleFieldUpdate
    ) -> Binding<Value> {
        Binding {
            value
        } set: { value in
            apply(field(value))
        }
    }
}
