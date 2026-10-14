namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class AuditForecastChecks
{
    public static void AuditForecastStartsInDraft()
    {
        var item = AuditForecast.NewAuditForecast("code-114", "name-134");
        Check.Equal(item.Status, AuditForecastStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditForecastMovesToClosed()
    {
        var item = AuditForecast.NewAuditForecast("code-114", "name-134");
        try
        {
            item.MoveTo(AuditForecastStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditForecastStatus.Closed, "status");
    }

    public static void AuditForecastRejectsMovesFromClosed()
    {
        var item = AuditForecast.NewAuditForecast("code-114", "name-134");
        item.Status = AuditForecastStatus.Closed;
        Check.Fails(() => item.MoveTo(AuditForecastStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AuditForecastStartsInDraft();
        AuditForecastMovesToClosed();
        AuditForecastRejectsMovesFromClosed();
    }
}
