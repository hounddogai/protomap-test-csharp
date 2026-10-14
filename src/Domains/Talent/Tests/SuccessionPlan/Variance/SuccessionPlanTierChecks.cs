namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanTierChecks
{
    public static void SuccessionPlanTierStartsInCreated()
    {
        var item = SuccessionPlanTier.NewSuccessionPlanTier("code-292", "title-27");
        Check.Equal(item.Status, SuccessionPlanTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanTierMovesToValidated()
    {
        var item = SuccessionPlanTier.NewSuccessionPlanTier("code-292", "title-27");
        try
        {
            item.MoveTo(SuccessionPlanTierStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanTierStatus.Validated, "status");
    }

    public static void SuccessionPlanTierRejectsMovesFromRetired()
    {
        var item = SuccessionPlanTier.NewSuccessionPlanTier("code-292", "title-27");
        item.Status = SuccessionPlanTierStatus.Retired;
        Check.Fails(() => item.MoveTo(SuccessionPlanTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanTierStartsInCreated();
        SuccessionPlanTierMovesToValidated();
        SuccessionPlanTierRejectsMovesFromRetired();
    }
}
