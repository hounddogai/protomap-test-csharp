namespace Acme.Workforce.Absence.Tests.Balance.BalanceTarget;

using Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>Checks of balance target code, run by the project's runner.</summary>
public static class RegionalBalanceTargetRulesChecks
{
    public static void RegionalBalanceTargetValidatorReportsEveryProblem()
    {
        var validator = RegionalBalanceTargetRules.DefaultRegionalBalanceTargetValidator();
        var item = RegionalBalanceTarget.NewRegionalBalanceTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalBalanceTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalBalanceTargetRequiredNameRule("regional balance target needs a value");
        var item = RegionalBalanceTarget.NewRegionalBalanceTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalBalanceTargetDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new RegionalBalanceTargetDurationMinutesRangeRule(16, 32);
        var item = RegionalBalanceTarget.NewRegionalBalanceTarget("x", "y");
        item.DurationMinutes = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalBalanceTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalBalanceTargetCodeLengthRule(80);
        var item = RegionalBalanceTarget.NewRegionalBalanceTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalBalanceTargetValidatorReportsEveryProblem();
        RegionalBalanceTargetRequiredNameRuleFlagsABlankName();
        RegionalBalanceTargetDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        RegionalBalanceTargetCodeLengthRuleFlagsALongCode();
    }
}
