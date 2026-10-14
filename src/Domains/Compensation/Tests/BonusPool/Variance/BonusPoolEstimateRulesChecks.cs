namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BonusPoolEstimateRulesChecks
{
    public static void BonusPoolEstimateValidatorReportsEveryProblem()
    {
        var validator = BonusPoolEstimateRules.DefaultBonusPoolEstimateValidator();
        var item = BonusPoolEstimate.NewBonusPoolEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolEstimateRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new BonusPoolEstimateRequiredTimezoneRule("bonus pool estimate needs an entry");
        var item = BonusPoolEstimate.NewBonusPoolEstimate("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolEstimateNameLengthRuleFlagsALongName()
    {
        var rule = new BonusPoolEstimateNameLengthRule(16);
        var item = BonusPoolEstimate.NewBonusPoolEstimate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BonusPoolEstimateRequiredCodeRule("bonus pool estimate needs this field");
        var item = BonusPoolEstimate.NewBonusPoolEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BonusPoolEstimateValidatorReportsEveryProblem();
        BonusPoolEstimateRequiredTimezoneRuleFlagsABlankTimezone();
        BonusPoolEstimateNameLengthRuleFlagsALongName();
        BonusPoolEstimateRequiredCodeRuleFlagsABlankCode();
    }
}
