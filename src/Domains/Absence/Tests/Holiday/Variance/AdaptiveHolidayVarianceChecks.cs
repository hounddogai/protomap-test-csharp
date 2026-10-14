namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveHolidayVarianceChecks
{
    public static void AdaptiveHolidayVarianceStartsInNew()
    {
        var item = AdaptiveHolidayVariance.NewAdaptiveHolidayVariance("code-800", "label-489");
        Check.Equal(item.Status, AdaptiveHolidayVarianceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveHolidayVarianceMovesToRunning()
    {
        var item = AdaptiveHolidayVariance.NewAdaptiveHolidayVariance("code-800", "label-489");
        try
        {
            item.MoveTo(AdaptiveHolidayVarianceStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveHolidayVarianceStatus.Running, "status");
    }

    public static void AdaptiveHolidayVarianceRejectsMovesFromFailed()
    {
        var item = AdaptiveHolidayVariance.NewAdaptiveHolidayVariance("code-800", "label-489");
        item.Status = AdaptiveHolidayVarianceStatus.Failed;
        Check.Fails(() => item.MoveTo(AdaptiveHolidayVarianceStatus.New), "move");
    }

    public static void RunAll()
    {
        AdaptiveHolidayVarianceStartsInNew();
        AdaptiveHolidayVarianceMovesToRunning();
        AdaptiveHolidayVarianceRejectsMovesFromFailed();
    }
}
