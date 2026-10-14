namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class PendingEquityGrantOverrideViewChecks
{
    public static void PendingEquityGrantOverrideViewShowsTheStatusInCapitals()
    {
        var item = PendingEquityGrantOverride.NewPendingEquityGrantOverride("code-522", "name-670");
        var view = PendingEquityGrantOverrideView.ToPendingEquityGrantOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingEquityGrantOverrideView.ToPendingEquityGrantOverrideViews(new List<PendingEquityGrantOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingEquityGrantOverrideViewShowsTheStatusInCapitals();
    }
}
