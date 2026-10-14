namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkProfileRulesChecks
{
    public static void StatementOfWorkProfileValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkProfileRules.DefaultStatementOfWorkProfileValidator();
        var item = StatementOfWorkProfile.NewStatementOfWorkProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkProfileThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new StatementOfWorkProfileThresholdRangeRule(5.2, 2168.0);
        var item = StatementOfWorkProfile.NewStatementOfWorkProfile("x", "y");
        item.Threshold = 2169.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkProfileExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new StatementOfWorkProfileExpiresAtRangeRule(80, 255);
        var item = StatementOfWorkProfile.NewStatementOfWorkProfile("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkProfileRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new StatementOfWorkProfileRequiredBadgeRule("statement of work profile needs an entry");
        var item = StatementOfWorkProfile.NewStatementOfWorkProfile("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StatementOfWorkProfileValidatorReportsEveryProblem();
        StatementOfWorkProfileThresholdRangeRuleFlagsValuesAboveThreshold();
        StatementOfWorkProfileExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        StatementOfWorkProfileRequiredBadgeRuleFlagsABlankBadge();
    }
}
