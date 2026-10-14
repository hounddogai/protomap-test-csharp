namespace Acme.Workforce.Compliance.Tests.Certification.Draft;

using Acme.Workforce.Compliance.Certification.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class CertificationScoreViewChecks
{
    public static void CertificationScoreViewShowsTheStatusInCapitals()
    {
        var item = CertificationScore.NewCertificationScore("code-371", "label-440");
        var view = CertificationScoreView.ToCertificationScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationScoreView.ToCertificationScoreViews(new List<CertificationScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationScoreViewShowsTheStatusInCapitals();
    }
}
