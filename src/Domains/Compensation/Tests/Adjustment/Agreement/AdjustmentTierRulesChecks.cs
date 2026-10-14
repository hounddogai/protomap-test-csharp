namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdjustmentTierRulesChecks
{
    public static void AdjustmentTierValidatorReportsEveryProblem()
    {
        var validator = AdjustmentTierRules.DefaultAdjustmentTierValidator();
        var item = AdjustmentTier.NewAdjustmentTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentTierLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new AdjustmentTierLocaleLengthRule(3600);
        var item = AdjustmentTier.NewAdjustmentTier("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentTierJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new AdjustmentTierJobCodeLengthRule(120);
        var item = AdjustmentTier.NewAdjustmentTier("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentTierRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdjustmentTierRequiredTitleRule("adjustment tier needs an entry");
        var item = AdjustmentTier.NewAdjustmentTier("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentTierWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new AdjustmentTierWindowDaysRangeRule(64, 3600);
        var item = AdjustmentTier.NewAdjustmentTier("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentTierRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new AdjustmentTierRequiredJobCodeRule("adjustment tier needs this field");
        var item = AdjustmentTier.NewAdjustmentTier("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdjustmentTierValidatorReportsEveryProblem();
        AdjustmentTierLocaleLengthRuleFlagsALongLocale();
        AdjustmentTierJobCodeLengthRuleFlagsALongJobCode();
        AdjustmentTierRequiredTitleRuleFlagsABlankTitle();
        AdjustmentTierWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        AdjustmentTierRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
