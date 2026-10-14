namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentBudgetViewChecks
{
    public static void AdaptiveGarnishmentBudgetViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveGarnishmentBudget.NewAdaptiveGarnishmentBudget("code-457", "title-736");
        var view = AdaptiveGarnishmentBudgetView.ToAdaptiveGarnishmentBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveGarnishmentBudgetView.ToAdaptiveGarnishmentBudgetViews(new List<AdaptiveGarnishmentBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentBudgetViewShowsTheStatusInCapitals();
    }
}
