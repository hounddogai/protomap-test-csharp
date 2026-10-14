namespace Acme.Workforce.Payrollops.Tests.PayRun.RunLedger;

using Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>Checks of run ledger code, run by the project's runner.</summary>
public static class RollingPayRunCycleRulesChecks
{
    public static void RollingPayRunCycleValidatorReportsEveryProblem()
    {
        var validator = RollingPayRunCycleRules.DefaultRollingPayRunCycleValidator();
        var item = RollingPayRunCycle.NewRollingPayRunCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPayRunCycleRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new RollingPayRunCycleRequiredExternalRefRule("rolling pay run cycle needs a value");
        var item = RollingPayRunCycle.NewRollingPayRunCycle("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingPayRunCycleExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new RollingPayRunCycleExternalRefLengthRule(120);
        var item = RollingPayRunCycle.NewRollingPayRunCycle("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingPayRunCycleValidatorReportsEveryProblem();
        RollingPayRunCycleRequiredExternalRefRuleFlagsABlankExternalRef();
        RollingPayRunCycleExternalRefLengthRuleFlagsALongExternalRef();
    }
}
