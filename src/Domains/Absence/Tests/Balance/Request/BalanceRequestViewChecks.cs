namespace Acme.Workforce.Absence.Tests.Balance.Request;

using Acme.Workforce.Absence.Balance.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class BalanceRequestViewChecks
{
    public static void BalanceRequestViewShowsTheStatusInCapitals()
    {
        var item = BalanceRequest.NewBalanceRequest("code-970", "name-361");
        var view = BalanceRequestView.ToBalanceRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceRequestView.ToBalanceRequestViews(new List<BalanceRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceRequestViewShowsTheStatusInCapitals();
    }
}
