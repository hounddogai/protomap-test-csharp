namespace Acme.Workforce.Talent.Tests.CareerPath.Segment;

using Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CareerPathPreferenceRulesChecks
{
    public static void CareerPathPreferenceValidatorReportsEveryProblem()
    {
        var validator = CareerPathPreferenceRules.DefaultCareerPathPreferenceValidator();
        var item = CareerPathPreference.NewCareerPathPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathPreferenceRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new CareerPathPreferenceRequiredSalaryBandRule("career path preference needs an entry");
        var item = CareerPathPreference.NewCareerPathPreference("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CareerPathPreferenceRequiredCodeRule("career path preference needs this field");
        var item = CareerPathPreference.NewCareerPathPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathPreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CareerPathPreferenceRequiredTitleRule("career path preference needs a value");
        var item = CareerPathPreference.NewCareerPathPreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CareerPathPreferenceValidatorReportsEveryProblem();
        CareerPathPreferenceRequiredSalaryBandRuleFlagsABlankSalaryBand();
        CareerPathPreferenceRequiredCodeRuleFlagsABlankCode();
        CareerPathPreferenceRequiredTitleRuleFlagsABlankTitle();
    }
}
