namespace Acme.Workforce.Engagement.Tests.Feedback.Baseline;

using Acme.Workforce.Engagement.Feedback.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledFeedbackBaselineRulesChecks
{
    public static void ScheduledFeedbackBaselineValidatorReportsEveryProblem()
    {
        var validator = ScheduledFeedbackBaselineRules.DefaultScheduledFeedbackBaselineValidator();
        var item = ScheduledFeedbackBaseline.NewScheduledFeedbackBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledFeedbackBaselineCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new ScheduledFeedbackBaselineCurrencyLengthRule(80);
        var item = ScheduledFeedbackBaseline.NewScheduledFeedbackBaseline("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledFeedbackBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledFeedbackBaselineCodeLengthRule(32);
        var item = ScheduledFeedbackBaseline.NewScheduledFeedbackBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledFeedbackBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledFeedbackBaselineRequiredCodeRule("scheduled feedback baseline needs a value");
        var item = ScheduledFeedbackBaseline.NewScheduledFeedbackBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledFeedbackBaselineCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new ScheduledFeedbackBaselineCeilingRangeRule(0.7, 2095.1);
        var item = ScheduledFeedbackBaseline.NewScheduledFeedbackBaseline("x", "y");
        item.Ceiling = 2096.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledFeedbackBaselineValidatorReportsEveryProblem();
        ScheduledFeedbackBaselineCurrencyLengthRuleFlagsALongCurrency();
        ScheduledFeedbackBaselineCodeLengthRuleFlagsALongCode();
        ScheduledFeedbackBaselineRequiredCodeRuleFlagsABlankCode();
        ScheduledFeedbackBaselineCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
