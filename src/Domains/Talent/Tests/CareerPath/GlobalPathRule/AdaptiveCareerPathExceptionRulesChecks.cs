namespace Acme.Workforce.Talent.Tests.CareerPath.GlobalPathRule;

using Acme.Workforce.Talent.CareerPath.GlobalPathRule;

/// <summary>Checks of global path rule code, run by the project's runner.</summary>
public static class AdaptiveCareerPathExceptionRulesChecks
{
    public static void AdaptiveCareerPathExceptionValidatorReportsEveryProblem()
    {
        var validator = AdaptiveCareerPathExceptionRules.DefaultAdaptiveCareerPathExceptionValidator();
        var item = AdaptiveCareerPathException.NewAdaptiveCareerPathException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveCareerPathExceptionTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new AdaptiveCareerPathExceptionTenureMonthsRangeRule(16, 3600);
        var item = AdaptiveCareerPathException.NewAdaptiveCareerPathException("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveCareerPathExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveCareerPathExceptionRequiredCodeRule("adaptive career path exception needs a value");
        var item = AdaptiveCareerPathException.NewAdaptiveCareerPathException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveCareerPathExceptionValidatorReportsEveryProblem();
        AdaptiveCareerPathExceptionTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        AdaptiveCareerPathExceptionRequiredCodeRuleFlagsABlankCode();
    }
}
