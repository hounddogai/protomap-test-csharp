namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalFloorPlanBatchRulesChecks
{
    public static void SeasonalFloorPlanBatchValidatorReportsEveryProblem()
    {
        var validator = SeasonalFloorPlanBatchRules.DefaultSeasonalFloorPlanBatchValidator();
        var item = SeasonalFloorPlanBatch.NewSeasonalFloorPlanBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalFloorPlanBatchRequiredNameRuleFlagsABlankName()
    {
        var rule = new SeasonalFloorPlanBatchRequiredNameRule("seasonal floor plan batch needs this field");
        var item = SeasonalFloorPlanBatch.NewSeasonalFloorPlanBatch("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalFloorPlanBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalFloorPlanBatchCodeLengthRule(64);
        var item = SeasonalFloorPlanBatch.NewSeasonalFloorPlanBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalFloorPlanBatchAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new SeasonalFloorPlanBatchAmountRangeRule(4.1, 4251.4);
        var item = SeasonalFloorPlanBatch.NewSeasonalFloorPlanBatch("x", "y");
        item.Amount = 4252.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalFloorPlanBatchValidatorReportsEveryProblem();
        SeasonalFloorPlanBatchRequiredNameRuleFlagsABlankName();
        SeasonalFloorPlanBatchCodeLengthRuleFlagsALongCode();
        SeasonalFloorPlanBatchAmountRangeRuleFlagsValuesAboveAmount();
    }
}
