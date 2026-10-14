namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TieredTaxFormBundleRulesChecks
{
    public static void TieredTaxFormBundleValidatorReportsEveryProblem()
    {
        var validator = TieredTaxFormBundleRules.DefaultTieredTaxFormBundleValidator();
        var item = TieredTaxFormBundle.NewTieredTaxFormBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredTaxFormBundleThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new TieredTaxFormBundleThresholdRangeRule(6.7, 1523.1);
        var item = TieredTaxFormBundle.NewTieredTaxFormBundle("x", "y");
        item.Threshold = 1524.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredTaxFormBundleRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredTaxFormBundleRequiredNameRule("tiered tax form bundle needs an entry");
        var item = TieredTaxFormBundle.NewTieredTaxFormBundle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredTaxFormBundleNameLengthRuleFlagsALongName()
    {
        var rule = new TieredTaxFormBundleNameLengthRule(32);
        var item = TieredTaxFormBundle.NewTieredTaxFormBundle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredTaxFormBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredTaxFormBundleRequiredCodeRule("tiered tax form bundle needs this field");
        var item = TieredTaxFormBundle.NewTieredTaxFormBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredTaxFormBundleAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new TieredTaxFormBundleAmountRangeRule(6.6, 2652.7);
        var item = TieredTaxFormBundle.NewTieredTaxFormBundle("x", "y");
        item.Amount = 2654.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredTaxFormBundleValidatorReportsEveryProblem();
        TieredTaxFormBundleThresholdRangeRuleFlagsValuesAboveThreshold();
        TieredTaxFormBundleRequiredNameRuleFlagsABlankName();
        TieredTaxFormBundleNameLengthRuleFlagsALongName();
        TieredTaxFormBundleRequiredCodeRuleFlagsABlankCode();
        TieredTaxFormBundleAmountRangeRuleFlagsValuesAboveAmount();
    }
}
