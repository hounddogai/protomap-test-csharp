namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RetroPayRuleViewChecks
{
    public static void RetroPayRuleViewShowsTheStatusInCapitals()
    {
        var item = RetroPayRule.NewRetroPayRule("code-574", "title-262");
        var view = RetroPayRuleView.ToRetroPayRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayRuleView.ToRetroPayRuleViews(new List<RetroPayRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayRuleViewShowsTheStatusInCapitals();
    }
}
