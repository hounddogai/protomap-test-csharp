namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SeasonalExceptionMetricChecks
{
    public static void SeasonalExceptionMetricStartsInRequested()
    {
        var item = SeasonalExceptionMetric.NewSeasonalExceptionMetric("code-386", "name-963");
        Check.Equal(item.Status, SeasonalExceptionMetricStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalExceptionMetricMovesToGranted()
    {
        var item = SeasonalExceptionMetric.NewSeasonalExceptionMetric("code-386", "name-963");
        try
        {
            item.MoveTo(SeasonalExceptionMetricStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalExceptionMetricStatus.Granted, "status");
    }

    public static void SeasonalExceptionMetricRejectsMovesFromExpired()
    {
        var item = SeasonalExceptionMetric.NewSeasonalExceptionMetric("code-386", "name-963");
        item.Status = SeasonalExceptionMetricStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalExceptionMetricStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalExceptionMetricStartsInRequested();
        SeasonalExceptionMetricMovesToGranted();
        SeasonalExceptionMetricRejectsMovesFromExpired();
    }
}
