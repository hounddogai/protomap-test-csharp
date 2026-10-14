namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class EngagementSlotViewChecks
{
    public static void EngagementSlotViewShowsTheStatusInCapitals()
    {
        var item = EngagementSlot.NewEngagementSlot("code-225", "label-764");
        var view = EngagementSlotView.ToEngagementSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementSlotView.ToEngagementSlotViews(new List<EngagementSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementSlotViewShowsTheStatusInCapitals();
    }
}
