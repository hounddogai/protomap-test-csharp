namespace Acme.Workforce.Absence.Tests.Balance.Request;

using Acme.Workforce.Absence.Balance.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CappedBalanceRequestViewChecks
{
    public static void CappedBalanceRequestViewShowsTheStatusInCapitals()
    {
        var item = CappedBalanceRequest.NewCappedBalanceRequest("code-716", "title-655");
        var view = CappedBalanceRequestView.ToCappedBalanceRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedBalanceRequestView.ToCappedBalanceRequestViews(new List<CappedBalanceRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedBalanceRequestViewShowsTheStatusInCapitals();
    }
}
