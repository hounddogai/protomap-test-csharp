namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LegacyAuditClaimViewChecks
{
    public static void LegacyAuditClaimViewShowsTheStatusInCapitals()
    {
        var item = LegacyAuditClaim.NewLegacyAuditClaim("code-931", "title-362");
        var view = LegacyAuditClaimView.ToLegacyAuditClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyAuditClaimView.ToLegacyAuditClaimViews(new List<LegacyAuditClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyAuditClaimViewShowsTheStatusInCapitals();
    }
}
