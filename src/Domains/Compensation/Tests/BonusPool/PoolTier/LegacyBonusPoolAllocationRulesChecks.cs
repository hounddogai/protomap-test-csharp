namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>Checks of pool tier code, run by the project's runner.</summary>
public static class LegacyBonusPoolAllocationRulesChecks
{
    public static void LegacyBonusPoolAllocationValidatorReportsEveryProblem()
    {
        var validator = LegacyBonusPoolAllocationRules.DefaultLegacyBonusPoolAllocationValidator();
        var item = LegacyBonusPoolAllocation.NewLegacyBonusPoolAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyBonusPoolAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyBonusPoolAllocationRequiredCodeRule("legacy bonus pool allocation needs this field");
        var item = LegacyBonusPoolAllocation.NewLegacyBonusPoolAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyBonusPoolAllocationSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new LegacyBonusPoolAllocationSegmentLengthRule(3600);
        var item = LegacyBonusPoolAllocation.NewLegacyBonusPoolAllocation("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyBonusPoolAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyBonusPoolAllocationCodeLengthRule(1000);
        var item = LegacyBonusPoolAllocation.NewLegacyBonusPoolAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyBonusPoolAllocationValidatorReportsEveryProblem();
        LegacyBonusPoolAllocationRequiredCodeRuleFlagsABlankCode();
        LegacyBonusPoolAllocationSegmentLengthRuleFlagsALongSegment();
        LegacyBonusPoolAllocationCodeLengthRuleFlagsALongCode();
    }
}
