namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class DefaultOccupancyEstimateChecks
{
    public static void DefaultOccupancyEstimateStartsInRequested()
    {
        var item = DefaultOccupancyEstimate.NewDefaultOccupancyEstimate("code-483", "label-972");
        Check.Equal(item.Status, DefaultOccupancyEstimateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultOccupancyEstimateMovesToGranted()
    {
        var item = DefaultOccupancyEstimate.NewDefaultOccupancyEstimate("code-483", "label-972");
        try
        {
            item.MoveTo(DefaultOccupancyEstimateStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultOccupancyEstimateStatus.Granted, "status");
    }

    public static void DefaultOccupancyEstimateRejectsMovesFromExpired()
    {
        var item = DefaultOccupancyEstimate.NewDefaultOccupancyEstimate("code-483", "label-972");
        item.Status = DefaultOccupancyEstimateStatus.Expired;
        Check.Fails(() => item.MoveTo(DefaultOccupancyEstimateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DefaultOccupancyEstimateStartsInRequested();
        DefaultOccupancyEstimateMovesToGranted();
        DefaultOccupancyEstimateRejectsMovesFromExpired();
    }
}
