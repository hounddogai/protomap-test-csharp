namespace Acme.Workforce.Compliance.Tests.Certification.CertificationForecast;

using Acme.Workforce.Compliance.Certification.CertificationForecast;

/// <summary>Checks of certification forecast code, run by the project's runner.</summary>
public static class ManualCertificationForecastChecks
{
    public static void ManualCertificationForecastStartsInPlanned()
    {
        var item = ManualCertificationForecast.NewManualCertificationForecast("code-125", "name-499");
        Check.Equal(item.Status, ManualCertificationForecastStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualCertificationForecastMovesToScheduled()
    {
        var item = ManualCertificationForecast.NewManualCertificationForecast("code-125", "name-499");
        try
        {
            item.MoveTo(ManualCertificationForecastStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualCertificationForecastStatus.Scheduled, "status");
    }

    public static void ManualCertificationForecastRejectsMovesFromCancelled()
    {
        var item = ManualCertificationForecast.NewManualCertificationForecast("code-125", "name-499");
        item.Status = ManualCertificationForecastStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ManualCertificationForecastStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ManualCertificationForecastStartsInPlanned();
        ManualCertificationForecastMovesToScheduled();
        ManualCertificationForecastRejectsMovesFromCancelled();
    }
}
