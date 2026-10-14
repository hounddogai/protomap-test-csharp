namespace Acme.Workforce.Compensation.Tests.BonusPool.Rule;

using Acme.Workforce.Compensation.BonusPool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class WeightedBonusPoolScoreChecks
{
    public static void WeightedBonusPoolScoreStartsInCreated()
    {
        var item = WeightedBonusPoolScore.NewWeightedBonusPoolScore("code-429", "label-208");
        Check.Equal(item.Status, WeightedBonusPoolScoreStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedBonusPoolScoreMovesToValidated()
    {
        var item = WeightedBonusPoolScore.NewWeightedBonusPoolScore("code-429", "label-208");
        try
        {
            item.MoveTo(WeightedBonusPoolScoreStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedBonusPoolScoreStatus.Validated, "status");
    }

    public static void WeightedBonusPoolScoreRejectsMovesFromRetired()
    {
        var item = WeightedBonusPoolScore.NewWeightedBonusPoolScore("code-429", "label-208");
        item.Status = WeightedBonusPoolScoreStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedBonusPoolScoreStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedBonusPoolScoreStartsInCreated();
        WeightedBonusPoolScoreMovesToValidated();
        WeightedBonusPoolScoreRejectsMovesFromRetired();
    }
}
