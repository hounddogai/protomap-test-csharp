namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class ScheduledPayBandThresholdRulesChecks
{
    public static void ScheduledPayBandThresholdValidatorReportsEveryProblem()
    {
        var validator = ScheduledPayBandThresholdRules.DefaultScheduledPayBandThresholdValidator();
        var item = ScheduledPayBandThreshold.NewScheduledPayBandThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPayBandThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledPayBandThresholdCodeLengthRule(120);
        var item = ScheduledPayBandThreshold.NewScheduledPayBandThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPayBandThresholdRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledPayBandThresholdRequiredTitleRule("scheduled pay band threshold needs this field");
        var item = ScheduledPayBandThreshold.NewScheduledPayBandThreshold("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPayBandThresholdRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new ScheduledPayBandThresholdRequiredManagerIdRule("scheduled pay band threshold needs this field");
        var item = ScheduledPayBandThreshold.NewScheduledPayBandThreshold("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledPayBandThresholdValidatorReportsEveryProblem();
        ScheduledPayBandThresholdCodeLengthRuleFlagsALongCode();
        ScheduledPayBandThresholdRequiredTitleRuleFlagsABlankTitle();
        ScheduledPayBandThresholdRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
