namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RetroPayProfileViewChecks
{
    public static void RetroPayProfileViewShowsTheStatusInCapitals()
    {
        var item = RetroPayProfile.NewRetroPayProfile("code-334", "title-488");
        var view = RetroPayProfileView.ToRetroPayProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayProfileView.ToRetroPayProfileViews(new List<RetroPayProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayProfileViewShowsTheStatusInCapitals();
    }
}
