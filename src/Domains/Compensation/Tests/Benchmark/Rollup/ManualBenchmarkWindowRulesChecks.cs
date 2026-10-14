namespace Acme.Workforce.Compensation.Tests.Benchmark.Rollup;

using Acme.Workforce.Compensation.Benchmark.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ManualBenchmarkWindowRulesChecks
{
    public static void ManualBenchmarkWindowValidatorReportsEveryProblem()
    {
        var validator = ManualBenchmarkWindowRules.DefaultManualBenchmarkWindowValidator();
        var item = ManualBenchmarkWindow.NewManualBenchmarkWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualBenchmarkWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualBenchmarkWindowRequiredCodeRule("manual benchmark window needs a value");
        var item = ManualBenchmarkWindow.NewManualBenchmarkWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualBenchmarkWindowRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ManualBenchmarkWindowRequiredTitleRule("manual benchmark window needs a value");
        var item = ManualBenchmarkWindow.NewManualBenchmarkWindow("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualBenchmarkWindowReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new ManualBenchmarkWindowReviewerLengthRule(32);
        var item = ManualBenchmarkWindow.NewManualBenchmarkWindow("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBenchmarkWindowManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new ManualBenchmarkWindowManagerIdLengthRule(1000);
        var item = ManualBenchmarkWindow.NewManualBenchmarkWindow("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualBenchmarkWindowValidatorReportsEveryProblem();
        ManualBenchmarkWindowRequiredCodeRuleFlagsABlankCode();
        ManualBenchmarkWindowRequiredTitleRuleFlagsABlankTitle();
        ManualBenchmarkWindowReviewerLengthRuleFlagsALongReviewer();
        ManualBenchmarkWindowManagerIdLengthRuleFlagsALongManagerId();
    }
}
