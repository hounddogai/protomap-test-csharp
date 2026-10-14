namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class GlobalVisitorAdjustmentViewChecks
{
    public static void GlobalVisitorAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = GlobalVisitorAdjustment.NewGlobalVisitorAdjustment("code-835", "label-68");
        var view = GlobalVisitorAdjustmentView.ToGlobalVisitorAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalVisitorAdjustmentView.ToGlobalVisitorAdjustmentViews(new List<GlobalVisitorAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalVisitorAdjustmentViewShowsTheStatusInCapitals();
    }
}
