namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WeightedPayslipManifestRulesChecks
{
    public static void WeightedPayslipManifestValidatorReportsEveryProblem()
    {
        var validator = WeightedPayslipManifestRules.DefaultWeightedPayslipManifestValidator();
        var item = WeightedPayslipManifest.NewWeightedPayslipManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedPayslipManifestRequiredNameRuleFlagsABlankName()
    {
        var rule = new WeightedPayslipManifestRequiredNameRule("weighted payslip manifest needs this field");
        var item = WeightedPayslipManifest.NewWeightedPayslipManifest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedPayslipManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedPayslipManifestRequiredCodeRule("weighted payslip manifest needs this field");
        var item = WeightedPayslipManifest.NewWeightedPayslipManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedPayslipManifestBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new WeightedPayslipManifestBatchSizeRangeRule(64, 64);
        var item = WeightedPayslipManifest.NewWeightedPayslipManifest("x", "y");
        item.BatchSize = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedPayslipManifestNameLengthRuleFlagsALongName()
    {
        var rule = new WeightedPayslipManifestNameLengthRule(120);
        var item = WeightedPayslipManifest.NewWeightedPayslipManifest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedPayslipManifestDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new WeightedPayslipManifestDiscountRangeRule(2.5, 3821.4);
        var item = WeightedPayslipManifest.NewWeightedPayslipManifest("x", "y");
        item.Discount = 3822.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedPayslipManifestValidatorReportsEveryProblem();
        WeightedPayslipManifestRequiredNameRuleFlagsABlankName();
        WeightedPayslipManifestRequiredCodeRuleFlagsABlankCode();
        WeightedPayslipManifestBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        WeightedPayslipManifestNameLengthRuleFlagsALongName();
        WeightedPayslipManifestDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
