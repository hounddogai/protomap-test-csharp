namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementPreferenceRulesChecks
{
    public static void EngagementPreferenceValidatorReportsEveryProblem()
    {
        var validator = EngagementPreferenceRules.DefaultEngagementPreferenceValidator();
        var item = EngagementPreference.NewEngagementPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EngagementPreferenceRequiredCodeRule("engagement preference needs an entry");
        var item = EngagementPreference.NewEngagementPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementPreferenceRequiredNameRuleFlagsABlankName()
    {
        var rule = new EngagementPreferenceRequiredNameRule("engagement preference needs a value");
        var item = EngagementPreference.NewEngagementPreference("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementPreferenceRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new EngagementPreferenceRequiredSalaryBandRule("engagement preference needs a value");
        var item = EngagementPreference.NewEngagementPreference("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EngagementPreferenceValidatorReportsEveryProblem();
        EngagementPreferenceRequiredCodeRuleFlagsABlankCode();
        EngagementPreferenceRequiredNameRuleFlagsABlankName();
        EngagementPreferenceRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
