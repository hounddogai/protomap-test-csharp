namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class FallbackPayBandMappingChecks
{
    public static void FallbackPayBandMappingStartsInNew()
    {
        var item = FallbackPayBandMapping.NewFallbackPayBandMapping("code-834", "title-211");
        Check.Equal(item.Status, FallbackPayBandMappingStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPayBandMappingMovesToDone()
    {
        var item = FallbackPayBandMapping.NewFallbackPayBandMapping("code-834", "title-211");
        try
        {
            item.MoveTo(FallbackPayBandMappingStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPayBandMappingStatus.Done, "status");
    }

    public static void FallbackPayBandMappingRejectsMovesFromFailed()
    {
        var item = FallbackPayBandMapping.NewFallbackPayBandMapping("code-834", "title-211");
        item.Status = FallbackPayBandMappingStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackPayBandMappingStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackPayBandMappingStartsInNew();
        FallbackPayBandMappingMovesToDone();
        FallbackPayBandMappingRejectsMovesFromFailed();
    }
}
