namespace Acme.Workforce.Absence.Tests.Balance.Threshold;

using Acme.Workforce.Absence.Balance.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BalanceAdjustmentViewChecks
{
    public static void BalanceAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = BalanceAdjustment.NewBalanceAdjustment("code-491", "title-863");
        var view = BalanceAdjustmentView.ToBalanceAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceAdjustmentView.ToBalanceAdjustmentViews(new List<BalanceAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceAdjustmentViewShowsTheStatusInCapitals();
    }
}
