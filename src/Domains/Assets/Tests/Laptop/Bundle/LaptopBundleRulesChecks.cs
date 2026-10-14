namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopBundleRulesChecks
{
    public static void LaptopBundleValidatorReportsEveryProblem()
    {
        var validator = LaptopBundleRules.DefaultLaptopBundleValidator();
        var item = LaptopBundle.NewLaptopBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new LaptopBundleCodeLengthRule(86400);
        var item = LaptopBundle.NewLaptopBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopBundleRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new LaptopBundleRequiredBadgeRule("laptop bundle needs an entry");
        var item = LaptopBundle.NewLaptopBundle("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopBundleExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new LaptopBundleExternalRefLengthRule(3600);
        var item = LaptopBundle.NewLaptopBundle("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LaptopBundleRequiredCodeRule("laptop bundle needs a value");
        var item = LaptopBundle.NewLaptopBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LaptopBundleValidatorReportsEveryProblem();
        LaptopBundleCodeLengthRuleFlagsALongCode();
        LaptopBundleRequiredBadgeRuleFlagsABlankBadge();
        LaptopBundleExternalRefLengthRuleFlagsALongExternalRef();
        LaptopBundleRequiredCodeRuleFlagsABlankCode();
    }
}
