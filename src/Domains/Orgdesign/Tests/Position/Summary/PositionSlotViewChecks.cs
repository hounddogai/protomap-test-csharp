namespace Acme.Workforce.Orgdesign.Tests.Position.Summary;

using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PositionSlotViewChecks
{
    public static void PositionSlotViewShowsTheStatusInCapitals()
    {
        var item = PositionSlot.NewPositionSlot("code-319", "name-598");
        var view = PositionSlotView.ToPositionSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionSlotView.ToPositionSlotViews(new List<PositionSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionSlotViewShowsTheStatusInCapitals();
    }
}
