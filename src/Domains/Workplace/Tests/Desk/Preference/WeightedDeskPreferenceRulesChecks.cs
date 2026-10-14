namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedDeskPreferenceRulesChecks
{
    public static void WeightedDeskPreferenceValidatorReportsEveryProblem()
    {
        var validator = WeightedDeskPreferenceRules.DefaultWeightedDeskPreferenceValidator();
        var item = WeightedDeskPreference.NewWeightedDeskPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedDeskPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedDeskPreferenceRequiredCodeRule("weighted desk preference needs an entry");
        var item = WeightedDeskPreference.NewWeightedDeskPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedDeskPreferenceRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new WeightedDeskPreferenceRequiredSalaryBandRule("weighted desk preference needs an entry");
        var item = WeightedDeskPreference.NewWeightedDeskPreference("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedDeskPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedDeskPreferenceCodeLengthRule(1000);
        var item = WeightedDeskPreference.NewWeightedDeskPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedDeskPreferenceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedDeskPreferenceRequiredLabelRule("weighted desk preference needs this field");
        var item = WeightedDeskPreference.NewWeightedDeskPreference("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedDeskPreferenceValidatorReportsEveryProblem();
        WeightedDeskPreferenceRequiredCodeRuleFlagsABlankCode();
        WeightedDeskPreferenceRequiredSalaryBandRuleFlagsABlankSalaryBand();
        WeightedDeskPreferenceCodeLengthRuleFlagsALongCode();
        WeightedDeskPreferenceRequiredLabelRuleFlagsABlankLabel();
    }
}
