namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalAuditClaimViewChecks
{
    public static void SeasonalAuditClaimViewShowsTheStatusInCapitals()
    {
        var item = SeasonalAuditClaim.NewSeasonalAuditClaim("code-665", "label-755");
        var view = SeasonalAuditClaimView.ToSeasonalAuditClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalAuditClaimView.ToSeasonalAuditClaimViews(new List<SeasonalAuditClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalAuditClaimViewShowsTheStatusInCapitals();
    }
}
