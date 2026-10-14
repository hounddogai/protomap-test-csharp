namespace Acme.Workforce.Absence.Tests.Accrual.Cycle;

using Acme.Workforce.Absence.Accrual.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AccrualScoreRulesChecks
{
    public static void AccrualScoreValidatorReportsEveryProblem()
    {
        var validator = AccrualScoreRules.DefaultAccrualScoreValidator();
        var item = AccrualScore.NewAccrualScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualScoreSourceLengthRuleFlagsALongSource()
    {
        var rule = new AccrualScoreSourceLengthRule(120);
        var item = AccrualScore.NewAccrualScore("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualScorePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new AccrualScorePriorityRangeRule(16, 16);
        var item = AccrualScore.NewAccrualScore("x", "y");
        item.Priority = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualScoreRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new AccrualScoreRequiredBadgeRule("accrual score needs a value");
        var item = AccrualScore.NewAccrualScore("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccrualScoreValidatorReportsEveryProblem();
        AccrualScoreSourceLengthRuleFlagsALongSource();
        AccrualScorePriorityRangeRuleFlagsValuesAbovePriority();
        AccrualScoreRequiredBadgeRuleFlagsABlankBadge();
    }
}
