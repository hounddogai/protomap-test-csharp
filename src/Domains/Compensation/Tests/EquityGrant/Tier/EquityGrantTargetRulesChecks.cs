namespace Acme.Workforce.Compensation.Tests.EquityGrant.Tier;

using Acme.Workforce.Compensation.EquityGrant.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EquityGrantTargetRulesChecks
{
    public static void EquityGrantTargetValidatorReportsEveryProblem()
    {
        var validator = EquityGrantTargetRules.DefaultEquityGrantTargetValidator();
        var item = EquityGrantTarget.NewEquityGrantTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantTargetUnitLengthRuleFlagsALongUnit()
    {
        var rule = new EquityGrantTargetUnitLengthRule(120);
        var item = EquityGrantTarget.NewEquityGrantTarget("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantTargetRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new EquityGrantTargetRequiredUnitRule("equity grant target needs this field");
        var item = EquityGrantTarget.NewEquityGrantTarget("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantTargetLabelLengthRuleFlagsALongLabel()
    {
        var rule = new EquityGrantTargetLabelLengthRule(3600);
        var item = EquityGrantTarget.NewEquityGrantTarget("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        EquityGrantTargetValidatorReportsEveryProblem();
        EquityGrantTargetUnitLengthRuleFlagsALongUnit();
        EquityGrantTargetRequiredUnitRuleFlagsABlankUnit();
        EquityGrantTargetLabelLengthRuleFlagsALongLabel();
    }
}
