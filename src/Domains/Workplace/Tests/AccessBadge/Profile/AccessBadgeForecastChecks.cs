namespace Acme.Workforce.Workplace.Tests.AccessBadge.Profile;

using Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AccessBadgeForecastChecks
{
    public static void AccessBadgeForecastStartsInRequested()
    {
        var item = AccessBadgeForecast.NewAccessBadgeForecast("code-542", "name-381");
        Check.Equal(item.Status, AccessBadgeForecastStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeForecastMovesToGranted()
    {
        var item = AccessBadgeForecast.NewAccessBadgeForecast("code-542", "name-381");
        try
        {
            item.MoveTo(AccessBadgeForecastStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeForecastStatus.Granted, "status");
    }

    public static void AccessBadgeForecastRejectsMovesFromExpired()
    {
        var item = AccessBadgeForecast.NewAccessBadgeForecast("code-542", "name-381");
        item.Status = AccessBadgeForecastStatus.Expired;
        Check.Fails(() => item.MoveTo(AccessBadgeForecastStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AccessBadgeForecastStartsInRequested();
        AccessBadgeForecastMovesToGranted();
        AccessBadgeForecastRejectsMovesFromExpired();
    }
}
