namespace Acme.Workforce.Compliance.Tests.Certification.CertificationAssignment;

using Acme.Workforce.Compliance.Certification.CertificationAssignment;

/// <summary>Checks of certification assignment code, run by the project's runner.</summary>
public static class TieredCertificationSummaryViewChecks
{
    public static void TieredCertificationSummaryViewShowsTheStatusInCapitals()
    {
        var item = TieredCertificationSummary.NewTieredCertificationSummary("code-471", "title-598");
        var view = TieredCertificationSummaryView.ToTieredCertificationSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredCertificationSummaryView.ToTieredCertificationSummaryViews(new List<TieredCertificationSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredCertificationSummaryViewShowsTheStatusInCapitals();
    }
}
