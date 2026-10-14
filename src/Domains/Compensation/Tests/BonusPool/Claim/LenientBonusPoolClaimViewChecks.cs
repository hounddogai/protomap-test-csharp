namespace Acme.Workforce.Compensation.Tests.BonusPool.Claim;

using Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientBonusPoolClaimViewChecks
{
    public static void LenientBonusPoolClaimViewShowsTheStatusInCapitals()
    {
        var item = LenientBonusPoolClaim.NewLenientBonusPoolClaim("code-6", "name-514");
        var view = LenientBonusPoolClaimView.ToLenientBonusPoolClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientBonusPoolClaimView.ToLenientBonusPoolClaimViews(new List<LenientBonusPoolClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientBonusPoolClaimViewShowsTheStatusInCapitals();
    }
}
