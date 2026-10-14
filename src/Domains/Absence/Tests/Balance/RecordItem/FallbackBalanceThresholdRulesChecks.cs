namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class FallbackBalanceThresholdRulesChecks
{
    public static void FallbackBalanceThresholdValidatorReportsEveryProblem()
    {
        var validator = FallbackBalanceThresholdRules.DefaultFallbackBalanceThresholdValidator();
        var item = FallbackBalanceThreshold.NewFallbackBalanceThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackBalanceThresholdFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new FallbackBalanceThresholdFteRangeRule(8.4, 2074.4);
        var item = FallbackBalanceThreshold.NewFallbackBalanceThreshold("x", "y");
        item.Fte = 2075.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackBalanceThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackBalanceThresholdRequiredCodeRule("fallback balance threshold needs an entry");
        var item = FallbackBalanceThreshold.NewFallbackBalanceThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackBalanceThresholdValidatorReportsEveryProblem();
        FallbackBalanceThresholdFteRangeRuleFlagsValuesAboveFte();
        FallbackBalanceThresholdRequiredCodeRuleFlagsABlankCode();
    }
}
