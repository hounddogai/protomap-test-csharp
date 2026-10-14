namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class LenientReorgSlotViewChecks
{
    public static void LenientReorgSlotViewShowsTheStatusInCapitals()
    {
        var item = LenientReorgSlot.NewLenientReorgSlot("code-830", "label-30");
        var view = LenientReorgSlotView.ToLenientReorgSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientReorgSlotView.ToLenientReorgSlotViews(new List<LenientReorgSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientReorgSlotViewShowsTheStatusInCapitals();
    }
}
