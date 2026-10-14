namespace Acme.Workforce.Compensation.Tests.PayBand.Summary;

using Acme.Workforce.Compensation.PayBand.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultPayBandEstimateChecks
{
    public static void DefaultPayBandEstimateStartsInOpen()
    {
        var item = DefaultPayBandEstimate.NewDefaultPayBandEstimate("code-510", "title-297");
        Check.Equal(item.Status, DefaultPayBandEstimateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPayBandEstimateMovesToResolved()
    {
        var item = DefaultPayBandEstimate.NewDefaultPayBandEstimate("code-510", "title-297");
        try
        {
            item.MoveTo(DefaultPayBandEstimateStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPayBandEstimateStatus.Resolved, "status");
    }

    public static void DefaultPayBandEstimateRejectsMovesFromArchived()
    {
        var item = DefaultPayBandEstimate.NewDefaultPayBandEstimate("code-510", "title-297");
        item.Status = DefaultPayBandEstimateStatus.Archived;
        Check.Fails(() => item.MoveTo(DefaultPayBandEstimateStatus.Open), "move");
    }

    public static void RunAll()
    {
        DefaultPayBandEstimateStartsInOpen();
        DefaultPayBandEstimateMovesToResolved();
        DefaultPayBandEstimateRejectsMovesFromArchived();
    }
}
