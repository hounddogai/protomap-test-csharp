namespace Acme.Workforce.Workplace.Tests.Desk.DeskProposal;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Checks of desk proposal code, run by the project's runner.</summary>
public static class FallbackDeskSlotViewChecks
{
    public static void FallbackDeskSlotViewShowsTheStatusInCapitals()
    {
        var item = FallbackDeskSlot.NewFallbackDeskSlot("code-171", "title-692");
        var view = FallbackDeskSlotView.ToFallbackDeskSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackDeskSlotView.ToFallbackDeskSlotViews(new List<FallbackDeskSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackDeskSlotViewShowsTheStatusInCapitals();
    }
}
