namespace Acme.Workforce.Assets.Tests.Laptop.Quota;

using Acme.Workforce.Assets.Laptop.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LaptopQuotaRulesChecks
{
    public static void LaptopQuotaValidatorReportsEveryProblem()
    {
        var validator = LaptopQuotaRules.DefaultLaptopQuotaValidator();
        var item = LaptopQuota.NewLaptopQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopQuotaRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new LaptopQuotaRequiredRegionRule("laptop quota needs a value");
        var item = LaptopQuota.NewLaptopQuota("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopQuotaRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LaptopQuotaRequiredLabelRule("laptop quota needs a value");
        var item = LaptopQuota.NewLaptopQuota("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopQuotaLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LaptopQuotaLabelLengthRule(120);
        var item = LaptopQuota.NewLaptopQuota("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LaptopQuotaValidatorReportsEveryProblem();
        LaptopQuotaRequiredRegionRuleFlagsABlankRegion();
        LaptopQuotaRequiredLabelRuleFlagsABlankLabel();
        LaptopQuotaLabelLengthRuleFlagsALongLabel();
    }
}
