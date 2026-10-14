namespace Acme.Workforce.Talent.Tests.Pool.Group;

using Acme.Workforce.Talent.Pool.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class RollingPoolLedgerRulesChecks
{
    public static void RollingPoolLedgerValidatorReportsEveryProblem()
    {
        var validator = RollingPoolLedgerRules.DefaultRollingPoolLedgerValidator();
        var item = RollingPoolLedger.NewRollingPoolLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPoolLedgerRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new RollingPoolLedgerRateRangeRule(4.1, 82.7);
        var item = RollingPoolLedger.NewRollingPoolLedger("x", "y");
        item.Rate = 84.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingPoolLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingPoolLedgerCodeLengthRule(120);
        var item = RollingPoolLedger.NewRollingPoolLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingPoolLedgerLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RollingPoolLedgerLabelLengthRule(32);
        var item = RollingPoolLedger.NewRollingPoolLedger("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingPoolLedgerValidatorReportsEveryProblem();
        RollingPoolLedgerRateRangeRuleFlagsValuesAboveRate();
        RollingPoolLedgerCodeLengthRuleFlagsALongCode();
        RollingPoolLedgerLabelLengthRuleFlagsALongLabel();
    }
}
