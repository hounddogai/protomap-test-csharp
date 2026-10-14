namespace Acme.Workforce.Compensation.Tests.BonusPool.Exception;

using Acme.Workforce.Compensation.BonusPool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class FallbackBonusPoolProfileRulesChecks
{
    public static void FallbackBonusPoolProfileValidatorReportsEveryProblem()
    {
        var validator = FallbackBonusPoolProfileRules.DefaultFallbackBonusPoolProfileValidator();
        var item = FallbackBonusPoolProfile.NewFallbackBonusPoolProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackBonusPoolProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackBonusPoolProfileRequiredCodeRule("fallback bonus pool profile needs an entry");
        var item = FallbackBonusPoolProfile.NewFallbackBonusPoolProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackBonusPoolProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackBonusPoolProfileCodeLengthRule(80);
        var item = FallbackBonusPoolProfile.NewFallbackBonusPoolProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackBonusPoolProfileBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new FallbackBonusPoolProfileBatchSizeRangeRule(16, 1000);
        var item = FallbackBonusPoolProfile.NewFallbackBonusPoolProfile("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackBonusPoolProfileRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackBonusPoolProfileRequiredTitleRule("fallback bonus pool profile needs an entry");
        var item = FallbackBonusPoolProfile.NewFallbackBonusPoolProfile("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackBonusPoolProfileValidatorReportsEveryProblem();
        FallbackBonusPoolProfileRequiredCodeRuleFlagsABlankCode();
        FallbackBonusPoolProfileCodeLengthRuleFlagsALongCode();
        FallbackBonusPoolProfileBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        FallbackBonusPoolProfileRequiredTitleRuleFlagsABlankTitle();
    }
}
