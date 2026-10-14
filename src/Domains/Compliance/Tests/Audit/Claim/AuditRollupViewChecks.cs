namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AuditRollupViewChecks
{
    public static void AuditRollupViewShowsTheStatusInCapitals()
    {
        var item = AuditRollup.NewAuditRollup("code-680", "title-837");
        var view = AuditRollupView.ToAuditRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditRollupView.ToAuditRollupViews(new List<AuditRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditRollupViewShowsTheStatusInCapitals();
    }
}
