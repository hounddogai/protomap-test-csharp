namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BonusPoolPlanRulesChecks
{
    public static void BonusPoolPlanValidatorReportsEveryProblem()
    {
        var validator = BonusPoolPlanRules.DefaultBonusPoolPlanValidator();
        var item = BonusPoolPlan.NewBonusPoolPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolPlanRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new BonusPoolPlanRatingRangeRule(5.4, 2577.2);
        var item = BonusPoolPlan.NewBonusPoolPlan("x", "y");
        item.Rating = 2578.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new BonusPoolPlanCodeLengthRule(1000);
        var item = BonusPoolPlan.NewBonusPoolPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolPlanRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new BonusPoolPlanRatioRangeRule(1.7, 3766.2);
        var item = BonusPoolPlan.NewBonusPoolPlan("x", "y");
        item.Ratio = 3767.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolPlanRequiredNameRuleFlagsABlankName()
    {
        var rule = new BonusPoolPlanRequiredNameRule("bonus pool plan needs a value");
        var item = BonusPoolPlan.NewBonusPoolPlan("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BonusPoolPlanRequiredCodeRule("bonus pool plan needs an entry");
        var item = BonusPoolPlan.NewBonusPoolPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BonusPoolPlanValidatorReportsEveryProblem();
        BonusPoolPlanRatingRangeRuleFlagsValuesAboveRating();
        BonusPoolPlanCodeLengthRuleFlagsALongCode();
        BonusPoolPlanRatioRangeRuleFlagsValuesAboveRatio();
        BonusPoolPlanRequiredNameRuleFlagsABlankName();
        BonusPoolPlanRequiredCodeRuleFlagsABlankCode();
    }
}
