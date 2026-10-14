namespace Acme.Workforce.Orgdesign.Tests.Span.Estimate;

using Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimarySpanEstimateChecks
{
    public static void PrimarySpanEstimateStartsInCreated()
    {
        var item = PrimarySpanEstimate.NewPrimarySpanEstimate("code-171", "label-964");
        Check.Equal(item.Status, PrimarySpanEstimateStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimarySpanEstimateMovesToPublished()
    {
        var item = PrimarySpanEstimate.NewPrimarySpanEstimate("code-171", "label-964");
        try
        {
            item.MoveTo(PrimarySpanEstimateStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimarySpanEstimateStatus.Published, "status");
    }

    public static void PrimarySpanEstimateRejectsMovesFromRetired()
    {
        var item = PrimarySpanEstimate.NewPrimarySpanEstimate("code-171", "label-964");
        item.Status = PrimarySpanEstimateStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimarySpanEstimateStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimarySpanEstimateStartsInCreated();
        PrimarySpanEstimateMovesToPublished();
        PrimarySpanEstimateRejectsMovesFromRetired();
    }
}
