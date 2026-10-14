namespace Acme.Workforce.Workplace.Tests.Desk.DeskProposal;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Checks of desk proposal code, run by the project's runner.</summary>
public static class CappedDeskAdjustmentViewChecks
{
    public static void CappedDeskAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = CappedDeskAdjustment.NewCappedDeskAdjustment("code-893", "label-499");
        var view = CappedDeskAdjustmentView.ToCappedDeskAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedDeskAdjustmentView.ToCappedDeskAdjustmentViews(new List<CappedDeskAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedDeskAdjustmentViewShowsTheStatusInCapitals();
    }
}
