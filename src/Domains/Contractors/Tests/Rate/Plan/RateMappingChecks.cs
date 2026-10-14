namespace Acme.Workforce.Contractors.Tests.Rate.Plan;

using Acme.Workforce.Contractors.Rate.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RateMappingChecks
{
    public static void RateMappingStartsInNew()
    {
        var item = RateMapping.NewRateMapping("code-173", "title-390");
        Check.Equal(item.Status, RateMappingStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateMappingMovesToDone()
    {
        var item = RateMapping.NewRateMapping("code-173", "title-390");
        try
        {
            item.MoveTo(RateMappingStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateMappingStatus.Done, "status");
    }

    public static void RateMappingRejectsMovesFromFailed()
    {
        var item = RateMapping.NewRateMapping("code-173", "title-390");
        item.Status = RateMappingStatus.Failed;
        Check.Fails(() => item.MoveTo(RateMappingStatus.New), "move");
    }

    public static void RunAll()
    {
        RateMappingStartsInNew();
        RateMappingMovesToDone();
        RateMappingRejectsMovesFromFailed();
    }
}
