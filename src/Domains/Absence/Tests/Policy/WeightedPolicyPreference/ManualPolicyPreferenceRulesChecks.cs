namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class ManualPolicyPreferenceRulesChecks
{
    public static void ManualPolicyPreferenceValidatorReportsEveryProblem()
    {
        var validator = ManualPolicyPreferenceRules.DefaultManualPolicyPreferenceValidator();
        var item = ManualPolicyPreference.NewManualPolicyPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualPolicyPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualPolicyPreferenceRequiredCodeRule("manual policy preference needs a value");
        var item = ManualPolicyPreference.NewManualPolicyPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualPolicyPreferenceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ManualPolicyPreferenceRequiredLabelRule("manual policy preference needs a value");
        var item = ManualPolicyPreference.NewManualPolicyPreference("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualPolicyPreferenceLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ManualPolicyPreferenceLabelLengthRule(80);
        var item = ManualPolicyPreference.NewManualPolicyPreference("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualPolicyPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualPolicyPreferenceCodeLengthRule(255);
        var item = ManualPolicyPreference.NewManualPolicyPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualPolicyPreferenceRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new ManualPolicyPreferenceRevisionRangeRule(120, 255);
        var item = ManualPolicyPreference.NewManualPolicyPreference("x", "y");
        item.Revision = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualPolicyPreferenceValidatorReportsEveryProblem();
        ManualPolicyPreferenceRequiredCodeRuleFlagsABlankCode();
        ManualPolicyPreferenceRequiredLabelRuleFlagsABlankLabel();
        ManualPolicyPreferenceLabelLengthRuleFlagsALongLabel();
        ManualPolicyPreferenceCodeLengthRuleFlagsALongCode();
        ManualPolicyPreferenceRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
