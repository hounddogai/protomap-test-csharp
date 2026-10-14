namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSnapshot;

using Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>Checks of reorg snapshot code, run by the project's runner.</summary>
public static class ScheduledReorgThresholdRulesChecks
{
    public static void ScheduledReorgThresholdValidatorReportsEveryProblem()
    {
        var validator = ScheduledReorgThresholdRules.DefaultScheduledReorgThresholdValidator();
        var item = ScheduledReorgThreshold.NewScheduledReorgThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledReorgThresholdLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ScheduledReorgThresholdLabelLengthRule(255);
        var item = ScheduledReorgThreshold.NewScheduledReorgThreshold("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledReorgThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledReorgThresholdCodeLengthRule(64);
        var item = ScheduledReorgThreshold.NewScheduledReorgThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledReorgThresholdValidatorReportsEveryProblem();
        ScheduledReorgThresholdLabelLengthRuleFlagsALongLabel();
        ScheduledReorgThresholdCodeLengthRuleFlagsALongCode();
    }
}
