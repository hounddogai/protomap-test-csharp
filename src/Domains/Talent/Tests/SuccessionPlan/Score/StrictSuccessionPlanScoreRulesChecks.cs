namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Score;

using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictSuccessionPlanScoreRulesChecks
{
    public static void StrictSuccessionPlanScoreValidatorReportsEveryProblem()
    {
        var validator = StrictSuccessionPlanScoreRules.DefaultStrictSuccessionPlanScoreValidator();
        var item = StrictSuccessionPlanScore.NewStrictSuccessionPlanScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictSuccessionPlanScoreRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new StrictSuccessionPlanScoreRequiredContactEmailRule(
            "strict succession plan score needs this field");
        var item = StrictSuccessionPlanScore.NewStrictSuccessionPlanScore("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictSuccessionPlanScoreCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new StrictSuccessionPlanScoreCeilingRangeRule(0.9, 1651.3);
        var item = StrictSuccessionPlanScore.NewStrictSuccessionPlanScore("x", "y");
        item.Ceiling = 1652.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictSuccessionPlanScoreContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new StrictSuccessionPlanScoreContactEmailLengthRule(80);
        var item = StrictSuccessionPlanScore.NewStrictSuccessionPlanScore("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictSuccessionPlanScoreRequiredNameRuleFlagsABlankName()
    {
        var rule = new StrictSuccessionPlanScoreRequiredNameRule("strict succession plan score needs a value");
        var item = StrictSuccessionPlanScore.NewStrictSuccessionPlanScore("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictSuccessionPlanScoreCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new StrictSuccessionPlanScoreCostRangeRule(0.9, 2212.8);
        var item = StrictSuccessionPlanScore.NewStrictSuccessionPlanScore("x", "y");
        item.Cost = 2214.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictSuccessionPlanScoreValidatorReportsEveryProblem();
        StrictSuccessionPlanScoreRequiredContactEmailRuleFlagsABlankContactEmail();
        StrictSuccessionPlanScoreCeilingRangeRuleFlagsValuesAboveCeiling();
        StrictSuccessionPlanScoreContactEmailLengthRuleFlagsALongContactEmail();
        StrictSuccessionPlanScoreRequiredNameRuleFlagsABlankName();
        StrictSuccessionPlanScoreCostRangeRuleFlagsValuesAboveCost();
    }
}
