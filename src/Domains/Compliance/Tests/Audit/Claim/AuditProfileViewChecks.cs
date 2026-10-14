namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AuditProfileViewChecks
{
    public static void AuditProfileViewShowsTheStatusInCapitals()
    {
        var item = AuditProfile.NewAuditProfile("code-735", "label-129");
        var view = AuditProfileView.ToAuditProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditProfileView.ToAuditProfileViews(new List<AuditProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditProfileViewShowsTheStatusInCapitals();
    }
}
