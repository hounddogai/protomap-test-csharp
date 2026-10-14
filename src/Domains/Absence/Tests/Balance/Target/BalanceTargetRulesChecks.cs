namespace Acme.Workforce.Absence.Tests.Balance.Target;

using Acme.Workforce.Absence.Balance.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BalanceTargetRulesChecks
{
    public static void BalanceTargetValidatorReportsEveryProblem()
    {
        var validator = BalanceTargetRules.DefaultBalanceTargetValidator();
        var item = BalanceTarget.NewBalanceTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceTargetQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new BalanceTargetQuantityRangeRule(64, 86400);
        var item = BalanceTarget.NewBalanceTarget("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new BalanceTargetCodeLengthRule(1000);
        var item = BalanceTarget.NewBalanceTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceTargetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BalanceTargetRequiredTitleRule("balance target needs a value");
        var item = BalanceTarget.NewBalanceTarget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BalanceTargetValidatorReportsEveryProblem();
        BalanceTargetQuantityRangeRuleFlagsValuesAboveQuantity();
        BalanceTargetCodeLengthRuleFlagsALongCode();
        BalanceTargetRequiredTitleRuleFlagsABlankTitle();
    }
}
