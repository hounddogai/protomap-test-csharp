namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LegacyDeskSlotViewChecks
{
    public static void LegacyDeskSlotViewShowsTheStatusInCapitals()
    {
        var item = LegacyDeskSlot.NewLegacyDeskSlot("code-321", "title-487");
        var view = LegacyDeskSlotView.ToLegacyDeskSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyDeskSlotView.ToLegacyDeskSlotViews(new List<LegacyDeskSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyDeskSlotViewShowsTheStatusInCapitals();
    }
}
