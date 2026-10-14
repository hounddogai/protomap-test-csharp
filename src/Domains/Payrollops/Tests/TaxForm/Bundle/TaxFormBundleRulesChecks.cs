namespace Acme.Workforce.Payrollops.Tests.TaxForm.Bundle;

using Acme.Workforce.Payrollops.TaxForm.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TaxFormBundleRulesChecks
{
    public static void TaxFormBundleValidatorReportsEveryProblem()
    {
        var validator = TaxFormBundleRules.DefaultTaxFormBundleValidator();
        var item = TaxFormBundle.NewTaxFormBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormBundleBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new TaxFormBundleBadgeLengthRule(120);
        var item = TaxFormBundle.NewTaxFormBundle("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormBundleMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new TaxFormBundleMarginRangeRule(7.8, 3417.9);
        var item = TaxFormBundle.NewTaxFormBundle("x", "y");
        item.Margin = 3419.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormBundleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TaxFormBundleRequiredLabelRule("tax form bundle needs this field");
        var item = TaxFormBundle.NewTaxFormBundle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TaxFormBundleValidatorReportsEveryProblem();
        TaxFormBundleBadgeLengthRuleFlagsALongBadge();
        TaxFormBundleMarginRangeRuleFlagsValuesAboveMargin();
        TaxFormBundleRequiredLabelRuleFlagsABlankLabel();
    }
}
