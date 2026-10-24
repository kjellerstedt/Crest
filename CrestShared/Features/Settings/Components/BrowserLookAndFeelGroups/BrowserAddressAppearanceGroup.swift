import SwiftUI

/// The address field's fill, border, and how it opens the command palette,
/// on every Space on this device.
struct BrowserAddressAppearanceGroup: View {
    var space: BrowserSpaceAppearance?
    var showsPreview = false

    @Bindable private var appearance = BrowserDeviceAppearanceStore.shared

    var body: some View {
        CrestSettingsGroup("Address field", settings: settings) {
            if showsPreview {
                BrowserLookAndFeelPreview(space: space, focus: .addressField)
            }
        } content: {
            CrestSettingSlider("Color fill", value: fill, readout: .percent(zero: "None", full: "Strong"))
            CrestSettingSlider("Border", value: border, readout: .percent(zero: "None", full: "Strong"))
            CrestSettingRow(
                "Accent outline while editing",
                setting: outline.resettable("Accent outline while editing")
            ) {
                Toggle("Accent outline while editing", isOn: outline.binding)
                    .labelsHidden()
            }
            CrestSettingRow(
                "Animate into command palette",
                setting: paletteMorph.resettable("Animate into command palette")
            ) {
                Toggle("Animate into command palette", isOn: paletteMorph.binding)
                    .labelsHidden()
            }
            CrestSettingRow(
                "Loading indicator color", setting: loadingColor.resettable("Loading indicator color")
            ) {
                Picker("Loading indicator color", selection: loadingColor.binding) {
                    Text("Accent").tag(BrowserAddressAppearance.LoadingColor.accent)
                    Text("Website").tag(BrowserAddressAppearance.LoadingColor.website)
                }
                .pickerStyle(.segmented)
                .labelsHidden()
                .fixedSize()
                .accessibilityLabel("Loading indicator color")
            }
        }
    }

    private var fill: CrestSettingValue<Double> {
        CrestSettingValue($appearance.address.fill, default: BrowserLookAndFeelDefaults.address.fill)
    }

    private var border: CrestSettingValue<Double> {
        CrestSettingValue($appearance.address.border, default: BrowserLookAndFeelDefaults.address.border)
    }

    private var outline: CrestSettingValue<Bool> {
        CrestSettingValue(
            $appearance.address.usesAccentWhenEditing,
            default: BrowserLookAndFeelDefaults.address.usesAccentWhenEditing)
    }

    private var paletteMorph: CrestSettingValue<Bool> {
        CrestSettingValue(
            $appearance.address.animatesIntoCommandPalette,
            default: BrowserLookAndFeelDefaults.address.animatesIntoCommandPalette)
    }

    private var loadingColor: CrestSettingValue<BrowserAddressAppearance.LoadingColor> {
        CrestSettingValue(
            $appearance.address.loadingColor,
            default: BrowserLookAndFeelDefaults.address.loadingColor)
    }

    private var settings: [CrestResettableSetting] {
        [
            fill.resettable("Color fill"),
            border.resettable("Border"),
            outline.resettable("Accent outline while editing"),
            paletteMorph.resettable("Animate into command palette"),
            loadingColor.resettable("Loading indicator color"),
        ]
    }
}
