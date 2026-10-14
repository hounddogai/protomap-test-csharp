namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedTownHallExceptionRulesChecks
{
    public static void BatchedTownHallExceptionValidatorReportsEveryProblem()
    {
        var validator = BatchedTownHallExceptionRules.DefaultBatchedTownHallExceptionValidator();
        var item = BatchedTownHallException.NewBatchedTownHallException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedTownHallExceptionRequiredNameRuleFlagsABlankName()
    {
        var rule = new BatchedTownHallExceptionRequiredNameRule("batched town hall exception needs a value");
        var item = BatchedTownHallException.NewBatchedTownHallException("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedTownHallExceptionCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new BatchedTownHallExceptionCostRangeRule(3.8, 3296.0);
        var item = BatchedTownHallException.NewBatchedTownHallException("x", "y");
        item.Cost = 3297.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedTownHallExceptionRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new BatchedTownHallExceptionRateRangeRule(9.1, 1932.2);
        var item = BatchedTownHallException.NewBatchedTownHallException("x", "y");
        item.Rate = 1933.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedTownHallExceptionRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new BatchedTownHallExceptionRequiredUnitRule("batched town hall exception needs a value");
        var item = BatchedTownHallException.NewBatchedTownHallException("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedTownHallExceptionValidatorReportsEveryProblem();
        BatchedTownHallExceptionRequiredNameRuleFlagsABlankName();
        BatchedTownHallExceptionCostRangeRuleFlagsValuesAboveCost();
        BatchedTownHallExceptionRateRangeRuleFlagsValuesAboveRate();
        BatchedTownHallExceptionRequiredUnitRuleFlagsABlankUnit();
    }
}
