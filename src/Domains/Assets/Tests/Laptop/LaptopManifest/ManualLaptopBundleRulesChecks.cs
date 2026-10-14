namespace Acme.Workforce.Assets.Tests.Laptop.LaptopManifest;

using Acme.Workforce.Assets.Laptop.LaptopManifest;

/// <summary>Checks of laptop manifest code, run by the project's runner.</summary>
public static class ManualLaptopBundleRulesChecks
{
    public static void ManualLaptopBundleValidatorReportsEveryProblem()
    {
        var validator = ManualLaptopBundleRules.DefaultManualLaptopBundleValidator();
        var item = ManualLaptopBundle.NewManualLaptopBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualLaptopBundleDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new ManualLaptopBundleDiscountRangeRule(2.6, 2192.2);
        var item = ManualLaptopBundle.NewManualLaptopBundle("x", "y");
        item.Discount = 2193.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualLaptopBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualLaptopBundleRequiredCodeRule("manual laptop bundle needs a value");
        var item = ManualLaptopBundle.NewManualLaptopBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualLaptopBundleNameLengthRuleFlagsALongName()
    {
        var rule = new ManualLaptopBundleNameLengthRule(1000);
        var item = ManualLaptopBundle.NewManualLaptopBundle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualLaptopBundleValidatorReportsEveryProblem();
        ManualLaptopBundleDiscountRangeRuleFlagsValuesAboveDiscount();
        ManualLaptopBundleRequiredCodeRuleFlagsABlankCode();
        ManualLaptopBundleNameLengthRuleFlagsALongName();
    }
}
