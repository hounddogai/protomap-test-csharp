namespace Acme.Workforce.Compliance.Tests.Audit.Digest;

using Acme.Workforce.Compliance.Audit.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AuditDigestViewChecks
{
    public static void AuditDigestViewShowsTheStatusInCapitals()
    {
        var item = AuditDigest.NewAuditDigest("code-324", "name-994");
        var view = AuditDigestView.ToAuditDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditDigestView.ToAuditDigestViews(new List<AuditDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditDigestViewShowsTheStatusInCapitals();
    }
}
