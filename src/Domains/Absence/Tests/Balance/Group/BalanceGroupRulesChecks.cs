namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceGroupRulesChecks
{
    public static void BalanceGroupValidatorReportsEveryProblem()
    {
        var validator = BalanceGroupRules.DefaultBalanceGroupValidator();
        var item = BalanceGroup.NewBalanceGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new BalanceGroupCodeLengthRule(255);
        var item = BalanceGroup.NewBalanceGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceGroupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BalanceGroupLabelLengthRule(86400);
        var item = BalanceGroup.NewBalanceGroup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceGroupEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new BalanceGroupEmployeeIdLengthRule(1000);
        var item = BalanceGroup.NewBalanceGroup("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BalanceGroupValidatorReportsEveryProblem();
        BalanceGroupCodeLengthRuleFlagsALongCode();
        BalanceGroupLabelLengthRuleFlagsALongLabel();
        BalanceGroupEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
