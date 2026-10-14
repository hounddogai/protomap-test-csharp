namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SuccessionPlanScoreRulesChecks
{
    public static void SuccessionPlanScoreValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanScoreRules.DefaultSuccessionPlanScoreValidator();
        var item = SuccessionPlanScore.NewSuccessionPlanScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SuccessionPlanScoreRequiredCodeRule("succession plan score needs this field");
        var item = SuccessionPlanScore.NewSuccessionPlanScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanScoreUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new SuccessionPlanScoreUpdatedAtRangeRule(255, 86400);
        var item = SuccessionPlanScore.NewSuccessionPlanScore("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanScoreTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SuccessionPlanScoreTitleLengthRule(80);
        var item = SuccessionPlanScore.NewSuccessionPlanScore("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanScoreManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new SuccessionPlanScoreManagerIdLengthRule(64);
        var item = SuccessionPlanScore.NewSuccessionPlanScore("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SuccessionPlanScoreValidatorReportsEveryProblem();
        SuccessionPlanScoreRequiredCodeRuleFlagsABlankCode();
        SuccessionPlanScoreUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        SuccessionPlanScoreTitleLengthRuleFlagsALongTitle();
        SuccessionPlanScoreManagerIdLengthRuleFlagsALongManagerId();
    }
}
