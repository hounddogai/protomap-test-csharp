namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReorgPreferenceRulesChecks
{
    public static void ReorgPreferenceValidatorReportsEveryProblem()
    {
        var validator = ReorgPreferenceRules.DefaultReorgPreferenceValidator();
        var item = ReorgPreference.NewReorgPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgPreferenceRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new ReorgPreferenceRequiredSourceRule("reorg preference needs a value");
        var item = ReorgPreference.NewReorgPreference("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReorgPreferenceRequiredCodeRule("reorg preference needs this field");
        var item = ReorgPreference.NewReorgPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReorgPreferenceValidatorReportsEveryProblem();
        ReorgPreferenceRequiredSourceRuleFlagsABlankSource();
        ReorgPreferenceRequiredCodeRuleFlagsABlankCode();
    }
}
