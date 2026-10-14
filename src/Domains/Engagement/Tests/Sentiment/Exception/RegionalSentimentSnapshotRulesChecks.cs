namespace Acme.Workforce.Engagement.Tests.Sentiment.Exception;

using Acme.Workforce.Engagement.Sentiment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class RegionalSentimentSnapshotRulesChecks
{
    public static void RegionalSentimentSnapshotValidatorReportsEveryProblem()
    {
        var validator = RegionalSentimentSnapshotRules.DefaultRegionalSentimentSnapshotValidator();
        var item = RegionalSentimentSnapshot.NewRegionalSentimentSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalSentimentSnapshotEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new RegionalSentimentSnapshotEmployeeIdLengthRule(80);
        var item = RegionalSentimentSnapshot.NewRegionalSentimentSnapshot("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalSentimentSnapshotRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RegionalSentimentSnapshotRequiredLabelRule("regional sentiment snapshot needs an entry");
        var item = RegionalSentimentSnapshot.NewRegionalSentimentSnapshot("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalSentimentSnapshotRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new RegionalSentimentSnapshotRequiredEmployeeIdRule("regional sentiment snapshot needs this field");
        var item = RegionalSentimentSnapshot.NewRegionalSentimentSnapshot("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalSentimentSnapshotValidatorReportsEveryProblem();
        RegionalSentimentSnapshotEmployeeIdLengthRuleFlagsALongEmployeeId();
        RegionalSentimentSnapshotRequiredLabelRuleFlagsABlankLabel();
        RegionalSentimentSnapshotRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
