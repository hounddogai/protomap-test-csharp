namespace Acme.Workforce.Absence.Tests.Accrual.Tier;

using Acme.Workforce.Absence.Accrual.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccrualClaimViewChecks
{
    public static void AccrualClaimViewShowsTheStatusInCapitals()
    {
        var item = AccrualClaim.NewAccrualClaim("code-223", "label-612");
        var view = AccrualClaimView.ToAccrualClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualClaimView.ToAccrualClaimViews(new List<AccrualClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualClaimViewShowsTheStatusInCapitals();
    }
}
