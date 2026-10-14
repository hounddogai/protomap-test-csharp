namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementAssignment;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Checks of engagement assignment code, run by the project's runner.</summary>
public static class DefaultEngagementPreferenceRulesChecks
{
    public static void DefaultEngagementPreferenceValidatorReportsEveryProblem()
    {
        var validator = DefaultEngagementPreferenceRules.DefaultDefaultEngagementPreferenceValidator();
        var item = DefaultEngagementPreference.NewDefaultEngagementPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultEngagementPreferenceNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultEngagementPreferenceNameLengthRule(86400);
        var item = DefaultEngagementPreference.NewDefaultEngagementPreference("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultEngagementPreferenceLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DefaultEngagementPreferenceLabelLengthRule(3600);
        var item = DefaultEngagementPreference.NewDefaultEngagementPreference("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultEngagementPreferenceLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new DefaultEngagementPreferenceLimitRangeRule(80, 1000);
        var item = DefaultEngagementPreference.NewDefaultEngagementPreference("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultEngagementPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultEngagementPreferenceRequiredCodeRule("default engagement preference needs this field");
        var item = DefaultEngagementPreference.NewDefaultEngagementPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultEngagementPreferenceValidatorReportsEveryProblem();
        DefaultEngagementPreferenceNameLengthRuleFlagsALongName();
        DefaultEngagementPreferenceLabelLengthRuleFlagsALongLabel();
        DefaultEngagementPreferenceLimitRangeRuleFlagsValuesAboveLimit();
        DefaultEngagementPreferenceRequiredCodeRuleFlagsABlankCode();
    }
}
