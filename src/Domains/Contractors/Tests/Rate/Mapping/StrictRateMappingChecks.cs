namespace Acme.Workforce.Contractors.Tests.Rate.Mapping;

using Acme.Workforce.Contractors.Rate.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class StrictRateMappingChecks
{
    public static void StrictRateMappingStartsInRequested()
    {
        var item = StrictRateMapping.NewStrictRateMapping("code-559", "name-625");
        Check.Equal(item.Status, StrictRateMappingStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictRateMappingMovesToGranted()
    {
        var item = StrictRateMapping.NewStrictRateMapping("code-559", "name-625");
        try
        {
            item.MoveTo(StrictRateMappingStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictRateMappingStatus.Granted, "status");
    }

    public static void StrictRateMappingRejectsMovesFromExpired()
    {
        var item = StrictRateMapping.NewStrictRateMapping("code-559", "name-625");
        item.Status = StrictRateMappingStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictRateMappingStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictRateMappingStartsInRequested();
        StrictRateMappingMovesToGranted();
        StrictRateMappingRejectsMovesFromExpired();
    }
}
