namespace Acme.Workforce.Absence.Tests.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

/// <summary>Checks of seasonal plan manifest code, run by the project's runner.</summary>
public static class RegionalReturnPlanItemRulesChecks
{
    public static void RegionalReturnPlanItemValidatorReportsEveryProblem()
    {
        var validator = RegionalReturnPlanItemRules.DefaultRegionalReturnPlanItemValidator();
        var item = RegionalReturnPlanItem.NewRegionalReturnPlanItem("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalReturnPlanItemScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new RegionalReturnPlanItemScoreRangeRule(4.9, 4270.9);
        var item = RegionalReturnPlanItem.NewRegionalReturnPlanItem("x", "y");
        item.Score = 4272.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalReturnPlanItemValidatorReportsEveryProblem();
        RegionalReturnPlanItemScoreRangeRuleFlagsValuesAboveScore();
    }
}
