namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LicenseSeatBudgetViewChecks
{
    public static void LicenseSeatBudgetViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatBudget.NewLicenseSeatBudget("code-441", "label-578");
        var view = LicenseSeatBudgetView.ToLicenseSeatBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatBudgetView.ToLicenseSeatBudgetViews(new List<LicenseSeatBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatBudgetViewShowsTheStatusInCapitals();
    }
}
