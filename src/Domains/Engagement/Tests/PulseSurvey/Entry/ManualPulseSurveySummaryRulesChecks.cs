namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Entry;

using Acme.Workforce.Engagement.PulseSurvey.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ManualPulseSurveySummaryRulesChecks
{
    public static void ManualPulseSurveySummaryValidatorReportsEveryProblem()
    {
        var validator = ManualPulseSurveySummaryRules.DefaultManualPulseSurveySummaryValidator();
        var item = ManualPulseSurveySummary.NewManualPulseSurveySummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualPulseSurveySummaryMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new ManualPulseSurveySummaryMaximumRangeRule(7.2, 68.4);
        var item = ManualPulseSurveySummary.NewManualPulseSurveySummary("x", "y");
        item.Maximum = 69.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualPulseSurveySummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualPulseSurveySummaryCodeLengthRule(120);
        var item = ManualPulseSurveySummary.NewManualPulseSurveySummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualPulseSurveySummaryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ManualPulseSurveySummaryLabelLengthRule(255);
        var item = ManualPulseSurveySummary.NewManualPulseSurveySummary("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualPulseSurveySummaryValidatorReportsEveryProblem();
        ManualPulseSurveySummaryMaximumRangeRuleFlagsValuesAboveMaximum();
        ManualPulseSurveySummaryCodeLengthRuleFlagsALongCode();
        ManualPulseSurveySummaryLabelLengthRuleFlagsALongLabel();
    }
}
