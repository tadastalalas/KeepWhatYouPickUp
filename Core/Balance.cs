namespace KeepWhatYouPickUp.Core;

// Fixed gameplay numbers (not player settings).
internal static class Balance
{
    // Contour on items above the player's skill (ARGB, red). Replaces vanilla's green focus contour.
    internal const uint UnusableItemContourColor = 0xFFFF0000;

    // "Stolen Equipment Chance" is set in percent; the daily roll needs 0..1.
    internal const float PercentDivisor = 100f;
}
