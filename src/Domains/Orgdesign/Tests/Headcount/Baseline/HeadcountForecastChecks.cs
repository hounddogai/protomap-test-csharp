namespace Acme.Workforce.Orgdesign.Tests.Headcount.Baseline;

using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class HeadcountForecastChecks
{
    public static void HeadcountForecastStartsInCreated()
    {
        var item = HeadcountForecast.NewHeadcountForecast("code-171", "name-528");
        Check.Equal(item.Status, HeadcountForecastStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountForecastMovesToRetired()
    {
        var item = HeadcountForecast.NewHeadcountForecast("code-171", "name-528");
        try
        {
            item.MoveTo(HeadcountForecastStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountForecastStatus.Retired, "status");
    }

    public static void HeadcountForecastRejectsMovesFromRetired()
    {
        var item = HeadcountForecast.NewHeadcountForecast("code-171", "name-528");
        item.Status = HeadcountForecastStatus.Retired;
        Check.Fails(() => item.MoveTo(HeadcountForecastStatus.Created), "move");
    }

    public static void RunAll()
    {
        HeadcountForecastStartsInCreated();
        HeadcountForecastMovesToRetired();
        HeadcountForecastRejectsMovesFromRetired();
    }
}
