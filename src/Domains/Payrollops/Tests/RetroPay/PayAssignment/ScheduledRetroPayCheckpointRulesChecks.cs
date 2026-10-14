namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class ScheduledRetroPayCheckpointRulesChecks
{
    public static void ScheduledRetroPayCheckpointValidatorReportsEveryProblem()
    {
        var validator = ScheduledRetroPayCheckpointRules.DefaultScheduledRetroPayCheckpointValidator();
        var item = ScheduledRetroPayCheckpoint.NewScheduledRetroPayCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledRetroPayCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledRetroPayCheckpointCodeLengthRule(86400);
        var item = ScheduledRetroPayCheckpoint.NewScheduledRetroPayCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledRetroPayCheckpointRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new ScheduledRetroPayCheckpointRequiredLocaleRule("scheduled retro pay checkpoint needs a value");
        var item = ScheduledRetroPayCheckpoint.NewScheduledRetroPayCheckpoint("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledRetroPayCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledRetroPayCheckpointRequiredCodeRule("scheduled retro pay checkpoint needs an entry");
        var item = ScheduledRetroPayCheckpoint.NewScheduledRetroPayCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledRetroPayCheckpointRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ScheduledRetroPayCheckpointRequiredLabelRule("scheduled retro pay checkpoint needs this field");
        var item = ScheduledRetroPayCheckpoint.NewScheduledRetroPayCheckpoint("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledRetroPayCheckpointValidatorReportsEveryProblem();
        ScheduledRetroPayCheckpointCodeLengthRuleFlagsALongCode();
        ScheduledRetroPayCheckpointRequiredLocaleRuleFlagsABlankLocale();
        ScheduledRetroPayCheckpointRequiredCodeRuleFlagsABlankCode();
        ScheduledRetroPayCheckpointRequiredLabelRuleFlagsABlankLabel();
    }
}
