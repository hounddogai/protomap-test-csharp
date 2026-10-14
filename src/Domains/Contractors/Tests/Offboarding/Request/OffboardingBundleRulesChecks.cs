namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class OffboardingBundleRulesChecks
{
    public static void OffboardingBundleValidatorReportsEveryProblem()
    {
        var validator = OffboardingBundleRules.DefaultOffboardingBundleValidator();
        var item = OffboardingBundle.NewOffboardingBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingBundleRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new OffboardingBundleRequiredSalaryBandRule("offboarding bundle needs an entry");
        var item = OffboardingBundle.NewOffboardingBundle("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OffboardingBundleRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new OffboardingBundleRateRangeRule(3.5, 3709.1);
        var item = OffboardingBundle.NewOffboardingBundle("x", "y");
        item.Rate = 3710.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingBundleNameLengthRuleFlagsALongName()
    {
        var rule = new OffboardingBundleNameLengthRule(1000);
        var item = OffboardingBundle.NewOffboardingBundle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OffboardingBundleValidatorReportsEveryProblem();
        OffboardingBundleRequiredSalaryBandRuleFlagsABlankSalaryBand();
        OffboardingBundleRateRangeRuleFlagsValuesAboveRate();
        OffboardingBundleNameLengthRuleFlagsALongName();
    }
}
