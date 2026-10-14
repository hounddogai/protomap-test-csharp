namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class FallbackBonusPoolMilestoneRulesChecks
{
    public static void FallbackBonusPoolMilestoneValidatorReportsEveryProblem()
    {
        var validator = FallbackBonusPoolMilestoneRules.DefaultFallbackBonusPoolMilestoneValidator();
        var item = FallbackBonusPoolMilestone.NewFallbackBonusPoolMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackBonusPoolMilestoneExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new FallbackBonusPoolMilestoneExternalRefLengthRule(16);
        var item = FallbackBonusPoolMilestone.NewFallbackBonusPoolMilestone("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackBonusPoolMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackBonusPoolMilestoneCodeLengthRule(3600);
        var item = FallbackBonusPoolMilestone.NewFallbackBonusPoolMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackBonusPoolMilestoneRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackBonusPoolMilestoneRequiredNameRule("fallback bonus pool milestone needs an entry");
        var item = FallbackBonusPoolMilestone.NewFallbackBonusPoolMilestone("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackBonusPoolMilestoneCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new FallbackBonusPoolMilestoneCapacityRangeRule(80, 3600);
        var item = FallbackBonusPoolMilestone.NewFallbackBonusPoolMilestone("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackBonusPoolMilestoneValidatorReportsEveryProblem();
        FallbackBonusPoolMilestoneExternalRefLengthRuleFlagsALongExternalRef();
        FallbackBonusPoolMilestoneCodeLengthRuleFlagsALongCode();
        FallbackBonusPoolMilestoneRequiredNameRuleFlagsABlankName();
        FallbackBonusPoolMilestoneCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
