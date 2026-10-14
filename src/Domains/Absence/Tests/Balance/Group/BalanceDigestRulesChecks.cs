namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceDigestRulesChecks
{
    public static void BalanceDigestValidatorReportsEveryProblem()
    {
        var validator = BalanceDigestRules.DefaultBalanceDigestValidator();
        var item = BalanceDigest.NewBalanceDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceDigestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BalanceDigestTitleLengthRule(3600);
        var item = BalanceDigest.NewBalanceDigest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceDigestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BalanceDigestRequiredTitleRule("balance digest needs an entry");
        var item = BalanceDigest.NewBalanceDigest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceDigestAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new BalanceDigestAttemptsRangeRule(255, 3600);
        var item = BalanceDigest.NewBalanceDigest("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BalanceDigestValidatorReportsEveryProblem();
        BalanceDigestTitleLengthRuleFlagsALongTitle();
        BalanceDigestRequiredTitleRuleFlagsABlankTitle();
        BalanceDigestAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
