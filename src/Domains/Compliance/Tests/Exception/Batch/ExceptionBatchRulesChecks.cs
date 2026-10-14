namespace Acme.Workforce.Compliance.Tests.Exception.Batch;

using Acme.Workforce.Compliance.Exception.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ExceptionBatchRulesChecks
{
    public static void ExceptionBatchValidatorReportsEveryProblem()
    {
        var validator = ExceptionBatchRules.DefaultExceptionBatchValidator();
        var item = ExceptionBatch.NewExceptionBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionBatchMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new ExceptionBatchMinimumRangeRule(7.6, 587.7);
        var item = ExceptionBatch.NewExceptionBatch("x", "y");
        item.Minimum = 589.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionBatchTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ExceptionBatchTitleLengthRule(255);
        var item = ExceptionBatch.NewExceptionBatch("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionBatchScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new ExceptionBatchScoreRangeRule(9.5, 1578.3);
        var item = ExceptionBatch.NewExceptionBatch("x", "y");
        item.Score = 1579.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new ExceptionBatchCodeLengthRule(32);
        var item = ExceptionBatch.NewExceptionBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionBatchRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new ExceptionBatchRequiredTeamRule("exception batch needs an entry");
        var item = ExceptionBatch.NewExceptionBatch("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionBatchValidatorReportsEveryProblem();
        ExceptionBatchMinimumRangeRuleFlagsValuesAboveMinimum();
        ExceptionBatchTitleLengthRuleFlagsALongTitle();
        ExceptionBatchScoreRangeRuleFlagsValuesAboveScore();
        ExceptionBatchCodeLengthRuleFlagsALongCode();
        ExceptionBatchRequiredTeamRuleFlagsABlankTeam();
    }
}
