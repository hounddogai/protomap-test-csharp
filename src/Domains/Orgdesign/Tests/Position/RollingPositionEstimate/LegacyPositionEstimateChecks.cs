namespace Acme.Workforce.Orgdesign.Tests.Position.RollingPositionEstimate;

using Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

/// <summary>Checks of rolling position estimate code, run by the project's runner.</summary>
public static class LegacyPositionEstimateChecks
{
    public static void LegacyPositionEstimateStartsInCreated()
    {
        var item = LegacyPositionEstimate.NewLegacyPositionEstimate("code-588", "label-249");
        Check.Equal(item.Status, LegacyPositionEstimateStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPositionEstimateMovesToPublished()
    {
        var item = LegacyPositionEstimate.NewLegacyPositionEstimate("code-588", "label-249");
        try
        {
            item.MoveTo(LegacyPositionEstimateStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPositionEstimateStatus.Published, "status");
    }

    public static void LegacyPositionEstimateRejectsMovesFromRetired()
    {
        var item = LegacyPositionEstimate.NewLegacyPositionEstimate("code-588", "label-249");
        item.Status = LegacyPositionEstimateStatus.Retired;
        Check.Fails(() => item.MoveTo(LegacyPositionEstimateStatus.Created), "move");
    }

    public static void RunAll()
    {
        LegacyPositionEstimateStartsInCreated();
        LegacyPositionEstimateMovesToPublished();
        LegacyPositionEstimateRejectsMovesFromRetired();
    }
}
