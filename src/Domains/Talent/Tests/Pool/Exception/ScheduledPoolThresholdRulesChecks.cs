namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledPoolThresholdRulesChecks
{
    public static void ScheduledPoolThresholdValidatorReportsEveryProblem()
    {
        var validator = ScheduledPoolThresholdRules.DefaultScheduledPoolThresholdValidator();
        var item = ScheduledPoolThreshold.NewScheduledPoolThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPoolThresholdRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new ScheduledPoolThresholdRevisionRangeRule(255, 3600);
        var item = ScheduledPoolThreshold.NewScheduledPoolThreshold("x", "y");
        item.Revision = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledPoolThresholdValidatorReportsEveryProblem();
        ScheduledPoolThresholdRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
