namespace Acme.Workforce.Workplace.Tests.Desk.RecordItem;

using Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DeskTierViewChecks
{
    public static void DeskTierViewShowsTheStatusInCapitals()
    {
        var item = DeskTier.NewDeskTier("code-790", "name-884");
        var view = DeskTierView.ToDeskTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskTierView.ToDeskTierViews(new List<DeskTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskTierViewShowsTheStatusInCapitals();
    }
}
