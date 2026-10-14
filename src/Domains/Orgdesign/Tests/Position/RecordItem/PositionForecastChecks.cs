namespace Acme.Workforce.Orgdesign.Tests.Position.RecordItem;

using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PositionForecastChecks
{
    public static void PositionForecastStartsInRequested()
    {
        var item = PositionForecast.NewPositionForecast("code-415", "label-75");
        Check.Equal(item.Status, PositionForecastStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionForecastMovesToGranted()
    {
        var item = PositionForecast.NewPositionForecast("code-415", "label-75");
        try
        {
            item.MoveTo(PositionForecastStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionForecastStatus.Granted, "status");
    }

    public static void PositionForecastRejectsMovesFromExpired()
    {
        var item = PositionForecast.NewPositionForecast("code-415", "label-75");
        item.Status = PositionForecastStatus.Expired;
        Check.Fails(() => item.MoveTo(PositionForecastStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PositionForecastStartsInRequested();
        PositionForecastMovesToGranted();
        PositionForecastRejectsMovesFromExpired();
    }
}
