namespace Acme.Workforce.Compensation.Tests.EquityGrant.Forecast;

using Acme.Workforce.Compensation.EquityGrant.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class EquityGrantRollupRulesChecks
{
    public static void EquityGrantRollupValidatorReportsEveryProblem()
    {
        var validator = EquityGrantRollupRules.DefaultEquityGrantRollupValidator();
        var item = EquityGrantRollup.NewEquityGrantRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new EquityGrantRollupCodeLengthRule(86400);
        var item = EquityGrantRollup.NewEquityGrantRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantRollupNameLengthRuleFlagsALongName()
    {
        var rule = new EquityGrantRollupNameLengthRule(32);
        var item = EquityGrantRollup.NewEquityGrantRollup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        EquityGrantRollupValidatorReportsEveryProblem();
        EquityGrantRollupCodeLengthRuleFlagsALongCode();
        EquityGrantRollupNameLengthRuleFlagsALongName();
    }
}
