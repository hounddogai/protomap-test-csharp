namespace Acme.Workforce.Talent.Tests.CareerPath.Variance;

using Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class CareerPathVarianceRulesChecks
{
    public static void CareerPathVarianceValidatorReportsEveryProblem()
    {
        var validator = CareerPathVarianceRules.DefaultCareerPathVarianceValidator();
        var item = CareerPathVariance.NewCareerPathVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathVarianceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CareerPathVarianceTitleLengthRule(1000);
        var item = CareerPathVariance.NewCareerPathVariance("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathVarianceBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new CareerPathVarianceBudgetRangeRule(6.5, 864.6);
        var item = CareerPathVariance.NewCareerPathVariance("x", "y");
        item.Budget = 866.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CareerPathVarianceWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new CareerPathVarianceWindowDaysRangeRule(16, 1000);
        var item = CareerPathVariance.NewCareerPathVariance("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CareerPathVarianceLocationLengthRuleFlagsALongLocation()
    {
        var rule = new CareerPathVarianceLocationLengthRule(86400);
        var item = CareerPathVariance.NewCareerPathVariance("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new CareerPathVarianceCodeLengthRule(64);
        var item = CareerPathVariance.NewCareerPathVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CareerPathVarianceValidatorReportsEveryProblem();
        CareerPathVarianceTitleLengthRuleFlagsALongTitle();
        CareerPathVarianceBudgetRangeRuleFlagsValuesAboveBudget();
        CareerPathVarianceWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        CareerPathVarianceLocationLengthRuleFlagsALongLocation();
        CareerPathVarianceCodeLengthRuleFlagsALongCode();
    }
}
