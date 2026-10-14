namespace Acme.Workforce.Absence.Tests.Policy.RollingPolicyOverride;

using Acme.Workforce.Absence.Policy.RollingPolicyOverride;

/// <summary>Checks of rolling policy override code, run by the project's runner.</summary>
public static class WeightedPolicyOverrideChecks
{
    public static void WeightedPolicyOverrideStartsInOpen()
    {
        var item = WeightedPolicyOverride.NewWeightedPolicyOverride("code-679", "name-782");
        Check.Equal(item.Status, WeightedPolicyOverrideStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedPolicyOverrideMovesToArchived()
    {
        var item = WeightedPolicyOverride.NewWeightedPolicyOverride("code-679", "name-782");
        try
        {
            item.MoveTo(WeightedPolicyOverrideStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedPolicyOverrideStatus.Archived, "status");
    }

    public static void WeightedPolicyOverrideRejectsMovesFromArchived()
    {
        var item = WeightedPolicyOverride.NewWeightedPolicyOverride("code-679", "name-782");
        item.Status = WeightedPolicyOverrideStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedPolicyOverrideStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedPolicyOverrideStartsInOpen();
        WeightedPolicyOverrideMovesToArchived();
        WeightedPolicyOverrideRejectsMovesFromArchived();
    }
}
