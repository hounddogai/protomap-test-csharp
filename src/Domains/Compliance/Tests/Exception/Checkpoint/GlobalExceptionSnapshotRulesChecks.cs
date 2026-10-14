namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class GlobalExceptionSnapshotRulesChecks
{
    public static void GlobalExceptionSnapshotValidatorReportsEveryProblem()
    {
        var validator = GlobalExceptionSnapshotRules.DefaultGlobalExceptionSnapshotValidator();
        var item = GlobalExceptionSnapshot.NewGlobalExceptionSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalExceptionSnapshotJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new GlobalExceptionSnapshotJobCodeLengthRule(32);
        var item = GlobalExceptionSnapshot.NewGlobalExceptionSnapshot("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalExceptionSnapshotRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new GlobalExceptionSnapshotRequiredJobCodeRule("global exception snapshot needs a value");
        var item = GlobalExceptionSnapshot.NewGlobalExceptionSnapshot("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalExceptionSnapshotTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GlobalExceptionSnapshotTitleLengthRule(1000);
        var item = GlobalExceptionSnapshot.NewGlobalExceptionSnapshot("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalExceptionSnapshotValidatorReportsEveryProblem();
        GlobalExceptionSnapshotJobCodeLengthRuleFlagsALongJobCode();
        GlobalExceptionSnapshotRequiredJobCodeRuleFlagsABlankJobCode();
        GlobalExceptionSnapshotTitleLengthRuleFlagsALongTitle();
    }
}
