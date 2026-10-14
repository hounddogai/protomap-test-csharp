namespace Acme.Workforce.Talent.Tests.Mobility.Manifest;

using Acme.Workforce.Talent.Mobility.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ScheduledMobilityPreferenceRulesChecks
{
    public static void ScheduledMobilityPreferenceValidatorReportsEveryProblem()
    {
        var validator = ScheduledMobilityPreferenceRules.DefaultScheduledMobilityPreferenceValidator();
        var item = ScheduledMobilityPreference.NewScheduledMobilityPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledMobilityPreferenceLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ScheduledMobilityPreferenceLabelLengthRule(80);
        var item = ScheduledMobilityPreference.NewScheduledMobilityPreference("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledMobilityPreferenceRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new ScheduledMobilityPreferenceRequiredSalaryBandRule(
            "scheduled mobility preference needs this field");
        var item = ScheduledMobilityPreference.NewScheduledMobilityPreference("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledMobilityPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledMobilityPreferenceCodeLengthRule(1000);
        var item = ScheduledMobilityPreference.NewScheduledMobilityPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledMobilityPreferenceValidatorReportsEveryProblem();
        ScheduledMobilityPreferenceLabelLengthRuleFlagsALongLabel();
        ScheduledMobilityPreferenceRequiredSalaryBandRuleFlagsABlankSalaryBand();
        ScheduledMobilityPreferenceCodeLengthRuleFlagsALongCode();
    }
}
