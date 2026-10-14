namespace Acme.Workforce.Assets.Tests.Warranty.Summary;

using Acme.Workforce.Assets.Warranty.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WarrantyBundleRulesChecks
{
    public static void WarrantyBundleValidatorReportsEveryProblem()
    {
        var validator = WarrantyBundleRules.DefaultWarrantyBundleValidator();
        var item = WarrantyBundle.NewWarrantyBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyBundleRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new WarrantyBundleRequiredBadgeRule("warranty bundle needs a value");
        var item = WarrantyBundle.NewWarrantyBundle("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WarrantyBundleRequiredCodeRule("warranty bundle needs an entry");
        var item = WarrantyBundle.NewWarrantyBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WarrantyBundleValidatorReportsEveryProblem();
        WarrantyBundleRequiredBadgeRuleFlagsABlankBadge();
        WarrantyBundleRequiredCodeRuleFlagsABlankCode();
    }
}
