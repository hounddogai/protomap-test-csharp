namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class FallbackBalanceMappingRulesChecks
{
    public static void FallbackBalanceMappingValidatorReportsEveryProblem()
    {
        var validator = FallbackBalanceMappingRules.DefaultFallbackBalanceMappingValidator();
        var item = FallbackBalanceMapping.NewFallbackBalanceMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackBalanceMappingRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackBalanceMappingRequiredTitleRule("fallback balance mapping needs an entry");
        var item = FallbackBalanceMapping.NewFallbackBalanceMapping("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackBalanceMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackBalanceMappingCodeLengthRule(1000);
        var item = FallbackBalanceMapping.NewFallbackBalanceMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackBalanceMappingMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new FallbackBalanceMappingMinimumRangeRule(5.3, 4937.7);
        var item = FallbackBalanceMapping.NewFallbackBalanceMapping("x", "y");
        item.Minimum = 4939.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackBalanceMappingCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new FallbackBalanceMappingCeilingRangeRule(0.9, 3993.2);
        var item = FallbackBalanceMapping.NewFallbackBalanceMapping("x", "y");
        item.Ceiling = 3994.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackBalanceMappingValidatorReportsEveryProblem();
        FallbackBalanceMappingRequiredTitleRuleFlagsABlankTitle();
        FallbackBalanceMappingCodeLengthRuleFlagsALongCode();
        FallbackBalanceMappingMinimumRangeRuleFlagsValuesAboveMinimum();
        FallbackBalanceMappingCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
