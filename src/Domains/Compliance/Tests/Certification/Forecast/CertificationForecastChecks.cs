namespace Acme.Workforce.Compliance.Tests.Certification.Forecast;

using Acme.Workforce.Compliance.Certification.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class CertificationForecastChecks
{
    public static void CertificationForecastStartsInDraft()
    {
        var item = CertificationForecast.NewCertificationForecast("code-421", "name-836");
        Check.Equal(item.Status, CertificationForecastStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationForecastMovesToSuspended()
    {
        var item = CertificationForecast.NewCertificationForecast("code-421", "name-836");
        try
        {
            item.MoveTo(CertificationForecastStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationForecastStatus.Suspended, "status");
    }

    public static void CertificationForecastRejectsMovesFromClosed()
    {
        var item = CertificationForecast.NewCertificationForecast("code-421", "name-836");
        item.Status = CertificationForecastStatus.Closed;
        Check.Fails(() => item.MoveTo(CertificationForecastStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CertificationForecastStartsInDraft();
        CertificationForecastMovesToSuspended();
        CertificationForecastRejectsMovesFromClosed();
    }
}
