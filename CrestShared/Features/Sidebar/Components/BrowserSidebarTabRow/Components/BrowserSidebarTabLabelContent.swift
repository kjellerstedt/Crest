import SwiftUI

/// The same title and favicon in the list and in the travelling component.
/// Keeping the native Label layout also preserves its baseline and icon gap.
struct BrowserSidebarTabLabelContent: View {
    let tab: TabStateModel
    let favicons: FaviconAssets
    let profileID: UUID
    let isSelected: Bool
    let isLoaded: Bool
    let metrics: BrowserSidebarTabRowMetrics
    var leadingInset: CGFloat = 0
    var restoreSavedLocation: (() -> Void)?
    var faviconPrimaryClick: (() -> Void)?
    /// Where the favicon and its indicator sit, for a row whose button tells a
    /// double-click on them from one on the title. Nil where nothing needs it.
    var iconFrameChanged: ((CGRect) -> Void)?
    var titleOpacity = 1.0
    var iconOffset: CGFloat = 0
    @Environment(\.browserInteractionCapabilities) private var capabilities
    /// The core whose page states name a tab whose page failed to load. None
    /// where no window supplies one, such as a drag preview.
    @Environment(CrestCore.self) private var core: CrestCore?
    let textScale: Double

    var body: some View {
        Label {
            Text(tab.shownTitle(in: isLoaded ? core?.state : nil))
                .modifier(BrowserSidebarDensityFont(scale: textScale, supportsTouch: capabilities.supportsTouch))
                .foregroundStyle(.primary)
                .lineLimit(1)
                .browserTabResidency(isLoaded: isLoaded)
                .opacity(titleOpacity)
        } icon: {
            HStack(spacing: 3) {
                BrowserSidebarTabFaviconContent(
                    tab: tab, favicons: favicons, profileID: profileID, metrics: metrics,
                    isProminent: isSelected, isLoaded: isLoaded,
                    iconScale: textScale
                )
                #if os(macOS)
                    .simultaneousGesture(
                        TapGesture().onEnded { faviconPrimaryClick?() },
                        including: faviconPrimaryClick == nil ? .none : .all
                    )
                #endif
                if tab.placement == .saved, tab.isAwayFromSavedAddress, let restoreSavedLocation {
                    BrowserTabSavedLocationIndicator(restore: restoreSavedLocation)
                        .browserTabResidency(isLoaded: isLoaded)
                }
            }
            #if os(macOS)
                .modifier(BrowserSidebarTabIconFrameReader(report: iconFrameChanged))
            #endif
            .offset(x: iconOffset)
        }
        .padding(.leading, leadingInset)
        .frame(maxWidth: .infinity, maxHeight: metrics.fillsRowHeight ? .infinity : nil, alignment: .leading)
        .contentShape(.rect)
    }
}

#if os(macOS)
    /// Reports the icon's frame in the activation button's space, and only where
    /// a button asked for it, so rows that do not tell clicks apart watch nothing.
    private struct BrowserSidebarTabIconFrameReader: ViewModifier {
        let report: ((CGRect) -> Void)?

        @ViewBuilder
        func body(content: Content) -> some View {
            if let report {
                content.onGeometryChange(for: CGRect.self) {
                    $0.frame(in: .named(BrowserSidebarTabActivationButton.coordinateSpace))
                } action: {
                    report($0)
                }
            } else {
                content
            }
        }
    }
#endif
