namespace Acme.Workforce.Workplace.Tests.Visitor.OverrideItem;

using Acme.Workforce.Workplace.Visitor.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PendingVisitorAdjustmentViewChecks
{
    public static void PendingVisitorAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = PendingVisitorAdjustment.NewPendingVisitorAdjustment("code-705", "title-869");
        var view = PendingVisitorAdjustmentView.ToPendingVisitorAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingVisitorAdjustmentView.ToPendingVisitorAdjustmentViews(new List<PendingVisitorAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingVisitorAdjustmentViewShowsTheStatusInCapitals();
    }
}
