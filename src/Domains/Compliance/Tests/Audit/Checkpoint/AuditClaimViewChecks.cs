namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AuditClaimViewChecks
{
    public static void AuditClaimViewShowsTheStatusInCapitals()
    {
        var item = AuditClaim.NewAuditClaim("code-761", "name-660");
        var view = AuditClaimView.ToAuditClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditClaimView.ToAuditClaimViews(new List<AuditClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditClaimViewShowsTheStatusInCapitals();
    }
}
