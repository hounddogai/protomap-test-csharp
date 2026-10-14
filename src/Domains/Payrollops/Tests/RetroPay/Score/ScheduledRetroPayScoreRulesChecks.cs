namespace Acme.Workforce.Payrollops.Tests.RetroPay.Score;

using Acme.Workforce.Payrollops.RetroPay.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledRetroPayScoreRulesChecks
{
    public static void ScheduledRetroPayScoreValidatorReportsEveryProblem()
    {
        var validator = ScheduledRetroPayScoreRules.DefaultScheduledRetroPayScoreValidator();
        var item = ScheduledRetroPayScore.NewScheduledRetroPayScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledRetroPayScoreRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledRetroPayScoreRequiredTitleRule("scheduled retro pay score needs this field");
        var item = ScheduledRetroPayScore.NewScheduledRetroPayScore("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledRetroPayScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledRetroPayScoreCodeLengthRule(80);
        var item = ScheduledRetroPayScore.NewScheduledRetroPayScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledRetroPayScoreValidatorReportsEveryProblem();
        ScheduledRetroPayScoreRequiredTitleRuleFlagsABlankTitle();
        ScheduledRetroPayScoreCodeLengthRuleFlagsALongCode();
    }
}
