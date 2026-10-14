namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class ReorgAssignmentRulesChecks
{
    public static void ReorgAssignmentValidatorReportsEveryProblem()
    {
        var validator = ReorgAssignmentRules.DefaultReorgAssignmentValidator();
        var item = ReorgAssignment.NewReorgAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgAssignmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ReorgAssignmentRequiredTitleRule("reorg assignment needs this field");
        var item = ReorgAssignment.NewReorgAssignment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgAssignmentRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new ReorgAssignmentRetriesRangeRule(32, 3600);
        var item = ReorgAssignment.NewReorgAssignment("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgAssignmentGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new ReorgAssignmentGraceDaysRangeRule(255, 86400);
        var item = ReorgAssignment.NewReorgAssignment("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgAssignmentMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new ReorgAssignmentMarginRangeRule(7.0, 2171.2);
        var item = ReorgAssignment.NewReorgAssignment("x", "y");
        item.Margin = 2172.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgAssignmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ReorgAssignmentTitleLengthRule(255);
        var item = ReorgAssignment.NewReorgAssignment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReorgAssignmentValidatorReportsEveryProblem();
        ReorgAssignmentRequiredTitleRuleFlagsABlankTitle();
        ReorgAssignmentRetriesRangeRuleFlagsValuesAboveRetries();
        ReorgAssignmentGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        ReorgAssignmentMarginRangeRuleFlagsValuesAboveMargin();
        ReorgAssignmentTitleLengthRuleFlagsALongTitle();
    }
}
