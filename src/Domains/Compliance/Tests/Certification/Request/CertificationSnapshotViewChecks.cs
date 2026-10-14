namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationSnapshotViewChecks
{
    public static void CertificationSnapshotViewShowsTheStatusInCapitals()
    {
        var item = CertificationSnapshot.NewCertificationSnapshot("code-42", "label-390");
        var view = CertificationSnapshotView.ToCertificationSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationSnapshotView.ToCertificationSnapshotViews(new List<CertificationSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationSnapshotViewShowsTheStatusInCapitals();
    }
}
