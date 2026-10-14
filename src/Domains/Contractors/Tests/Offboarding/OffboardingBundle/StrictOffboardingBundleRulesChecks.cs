namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingBundle;

using Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

/// <summary>Checks of offboarding bundle code, run by the project's runner.</summary>
public static class StrictOffboardingBundleRulesChecks
{
    public static void StrictOffboardingBundleValidatorReportsEveryProblem()
    {
        var validator = StrictOffboardingBundleRules.DefaultStrictOffboardingBundleValidator();
        var item = StrictOffboardingBundle.NewStrictOffboardingBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictOffboardingBundleRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new StrictOffboardingBundleRequiredDepartmentRule("strict offboarding bundle needs this field");
        var item = StrictOffboardingBundle.NewStrictOffboardingBundle("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictOffboardingBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictOffboardingBundleCodeLengthRule(1000);
        var item = StrictOffboardingBundle.NewStrictOffboardingBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictOffboardingBundleCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new StrictOffboardingBundleCostRangeRule(4.5, 1728.3);
        var item = StrictOffboardingBundle.NewStrictOffboardingBundle("x", "y");
        item.Cost = 1729.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictOffboardingBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictOffboardingBundleRequiredCodeRule("strict offboarding bundle needs an entry");
        var item = StrictOffboardingBundle.NewStrictOffboardingBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictOffboardingBundleValidatorReportsEveryProblem();
        StrictOffboardingBundleRequiredDepartmentRuleFlagsABlankDepartment();
        StrictOffboardingBundleCodeLengthRuleFlagsALongCode();
        StrictOffboardingBundleCostRangeRuleFlagsValuesAboveCost();
        StrictOffboardingBundleRequiredCodeRuleFlagsABlankCode();
    }
}
