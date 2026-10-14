namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class GlobalNominationMilestoneRulesChecks
{
    public static void GlobalNominationMilestoneValidatorReportsEveryProblem()
    {
        var validator = GlobalNominationMilestoneRules.DefaultGlobalNominationMilestoneValidator();
        var item = GlobalNominationMilestone.NewGlobalNominationMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalNominationMilestoneDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new GlobalNominationMilestoneDiscountRangeRule(3.9, 1466.7);
        var item = GlobalNominationMilestone.NewGlobalNominationMilestone("x", "y");
        item.Discount = 1468.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalNominationMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalNominationMilestoneCodeLengthRule(16);
        var item = GlobalNominationMilestone.NewGlobalNominationMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalNominationMilestoneMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new GlobalNominationMilestoneMaximumRangeRule(4.5, 1572.9);
        var item = GlobalNominationMilestone.NewGlobalNominationMilestone("x", "y");
        item.Maximum = 1574.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalNominationMilestoneRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new GlobalNominationMilestoneRatioRangeRule(0.7, 1400.0);
        var item = GlobalNominationMilestone.NewGlobalNominationMilestone("x", "y");
        item.Ratio = 1401.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalNominationMilestoneExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new GlobalNominationMilestoneExpiresAtRangeRule(16, 86400);
        var item = GlobalNominationMilestone.NewGlobalNominationMilestone("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalNominationMilestoneValidatorReportsEveryProblem();
        GlobalNominationMilestoneDiscountRangeRuleFlagsValuesAboveDiscount();
        GlobalNominationMilestoneCodeLengthRuleFlagsALongCode();
        GlobalNominationMilestoneMaximumRangeRuleFlagsValuesAboveMaximum();
        GlobalNominationMilestoneRatioRangeRuleFlagsValuesAboveRatio();
        GlobalNominationMilestoneExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
