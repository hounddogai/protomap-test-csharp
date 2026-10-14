namespace Acme.Workforce.Talent.Tests.CareerPath.Plan;

using Acme.Workforce.Talent.CareerPath.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class WeightedCareerPathOverrideChecks
{
    public static void WeightedCareerPathOverrideStartsInProposed()
    {
        var item = WeightedCareerPathOverride.NewWeightedCareerPathOverride("code-915", "title-538");
        Check.Equal(item.Status, WeightedCareerPathOverrideStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedCareerPathOverrideMovesToApproved()
    {
        var item = WeightedCareerPathOverride.NewWeightedCareerPathOverride("code-915", "title-538");
        try
        {
            item.MoveTo(WeightedCareerPathOverrideStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedCareerPathOverrideStatus.Approved, "status");
    }

    public static void WeightedCareerPathOverrideRejectsMovesFromWithdrawn()
    {
        var item = WeightedCareerPathOverride.NewWeightedCareerPathOverride("code-915", "title-538");
        item.Status = WeightedCareerPathOverrideStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedCareerPathOverrideStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedCareerPathOverrideStartsInProposed();
        WeightedCareerPathOverrideMovesToApproved();
        WeightedCareerPathOverrideRejectsMovesFromWithdrawn();
    }
}
