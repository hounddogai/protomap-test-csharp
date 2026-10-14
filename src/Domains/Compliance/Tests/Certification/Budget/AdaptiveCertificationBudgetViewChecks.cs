namespace Acme.Workforce.Compliance.Tests.Certification.Budget;

using Acme.Workforce.Compliance.Certification.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AdaptiveCertificationBudgetViewChecks
{
    public static void AdaptiveCertificationBudgetViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveCertificationBudget.NewAdaptiveCertificationBudget("code-954", "name-148");
        var view = AdaptiveCertificationBudgetView.ToAdaptiveCertificationBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveCertificationBudgetView.ToAdaptiveCertificationBudgetViews(new List<AdaptiveCertificationBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveCertificationBudgetViewShowsTheStatusInCapitals();
    }
}
