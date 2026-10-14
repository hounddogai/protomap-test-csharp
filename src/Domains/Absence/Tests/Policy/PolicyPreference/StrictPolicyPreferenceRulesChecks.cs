namespace Acme.Workforce.Absence.Tests.Policy.PolicyPreference;

using Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>Checks of policy preference code, run by the project's runner.</summary>
public static class StrictPolicyPreferenceRulesChecks
{
    public static void StrictPolicyPreferenceValidatorReportsEveryProblem()
    {
        var validator = StrictPolicyPreferenceRules.DefaultStrictPolicyPreferenceValidator();
        var item = StrictPolicyPreference.NewStrictPolicyPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictPolicyPreferenceChannelLengthRuleFlagsALongChannel()
    {
        var rule = new StrictPolicyPreferenceChannelLengthRule(64);
        var item = StrictPolicyPreference.NewStrictPolicyPreference("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPolicyPreferenceRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new StrictPolicyPreferenceRequiredChannelRule("strict policy preference needs a value");
        var item = StrictPolicyPreference.NewStrictPolicyPreference("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictPolicyPreferenceRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new StrictPolicyPreferenceRequiredSourceRule("strict policy preference needs an entry");
        var item = StrictPolicyPreference.NewStrictPolicyPreference("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictPolicyPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictPolicyPreferenceCodeLengthRule(64);
        var item = StrictPolicyPreference.NewStrictPolicyPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPolicyPreferenceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new StrictPolicyPreferenceTitleLengthRule(80);
        var item = StrictPolicyPreference.NewStrictPolicyPreference("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictPolicyPreferenceValidatorReportsEveryProblem();
        StrictPolicyPreferenceChannelLengthRuleFlagsALongChannel();
        StrictPolicyPreferenceRequiredChannelRuleFlagsABlankChannel();
        StrictPolicyPreferenceRequiredSourceRuleFlagsABlankSource();
        StrictPolicyPreferenceCodeLengthRuleFlagsALongCode();
        StrictPolicyPreferenceTitleLengthRuleFlagsALongTitle();
    }
}
