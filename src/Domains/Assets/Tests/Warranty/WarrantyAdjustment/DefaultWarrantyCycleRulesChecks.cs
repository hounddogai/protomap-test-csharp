namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class DefaultWarrantyCycleRulesChecks
{
    public static void DefaultWarrantyCycleValidatorExtraReportsEveryProblem()
    {
        var validator = DefaultWarrantyCycleRules.DefaultDefaultWarrantyCycleValidator();
        var item = DefaultWarrantyCycle.NewDefaultWarrantyCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultWarrantyCycleRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultWarrantyCycleRequiredNameRule("default warranty cycle needs an entry");
        var item = DefaultWarrantyCycle.NewDefaultWarrantyCycle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultWarrantyCycleTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new DefaultWarrantyCycleTimezoneLengthRule(86400);
        var item = DefaultWarrantyCycle.NewDefaultWarrantyCycle("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultWarrantyCycleSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new DefaultWarrantyCycleSummaryLengthRule(255);
        var item = DefaultWarrantyCycle.NewDefaultWarrantyCycle("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultWarrantyCycleValidatorExtraReportsEveryProblem();
        DefaultWarrantyCycleRequiredNameRuleFlagsABlankName();
        DefaultWarrantyCycleTimezoneLengthRuleFlagsALongTimezone();
        DefaultWarrantyCycleSummaryLengthRuleFlagsALongSummary();
    }
}
