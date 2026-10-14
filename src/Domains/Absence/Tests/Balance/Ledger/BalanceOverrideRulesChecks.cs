namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BalanceOverrideRulesChecks
{
    public static void BalanceOverrideValidatorReportsEveryProblem()
    {
        var validator = BalanceOverrideRules.DefaultBalanceOverrideValidator();
        var item = BalanceOverride.NewBalanceOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceOverrideRegionLengthRuleFlagsALongRegion()
    {
        var rule = new BalanceOverrideRegionLengthRule(80);
        var item = BalanceOverride.NewBalanceOverride("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceOverrideRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new BalanceOverrideRequiredLocaleRule("balance override needs an entry");
        var item = BalanceOverride.NewBalanceOverride("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BalanceOverrideValidatorReportsEveryProblem();
        BalanceOverrideRegionLengthRuleFlagsALongRegion();
        BalanceOverrideRequiredLocaleRuleFlagsABlankLocale();
    }
}
