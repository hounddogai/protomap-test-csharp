namespace Acme.Workforce.Talent.Tests.CareerPath.PathDigest;

using Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>Checks of path digest code, run by the project's runner.</summary>
public static class CareerPathEstimateChecks
{
    public static void CareerPathEstimateStartsInRequested()
    {
        var item = CareerPathEstimate.NewCareerPathEstimate("code-680", "name-611");
        Check.Equal(item.Status, CareerPathEstimateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathEstimateMovesToGranted()
    {
        var item = CareerPathEstimate.NewCareerPathEstimate("code-680", "name-611");
        try
        {
            item.MoveTo(CareerPathEstimateStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathEstimateStatus.Granted, "status");
    }

    public static void CareerPathEstimateRejectsMovesFromExpired()
    {
        var item = CareerPathEstimate.NewCareerPathEstimate("code-680", "name-611");
        item.Status = CareerPathEstimateStatus.Expired;
        Check.Fails(() => item.MoveTo(CareerPathEstimateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CareerPathEstimateStartsInRequested();
        CareerPathEstimateMovesToGranted();
        CareerPathEstimateRejectsMovesFromExpired();
    }
}
