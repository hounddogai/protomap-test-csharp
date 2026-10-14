namespace Acme.Workforce.Compliance.Tests.Certification.Threshold;

using Acme.Workforce.Compliance.Certification.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class TieredCertificationCheckpointViewChecks
{
    public static void TieredCertificationCheckpointViewShowsTheStatusInCapitals()
    {
        var item = TieredCertificationCheckpoint.NewTieredCertificationCheckpoint("code-841", "name-481");
        var view = TieredCertificationCheckpointView.ToTieredCertificationCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredCertificationCheckpointView.ToTieredCertificationCheckpointViews(new List<TieredCertificationCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredCertificationCheckpointViewShowsTheStatusInCapitals();
    }
}
