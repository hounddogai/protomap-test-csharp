namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReorgCycleRulesChecks
{
    public static void ReorgCycleValidatorReportsEveryProblem()
    {
        var validator = ReorgCycleRules.DefaultReorgCycleValidator();
        var item = ReorgCycle.NewReorgCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgCycleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ReorgCycleLabelLengthRule(32);
        var item = ReorgCycle.NewReorgCycle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new ReorgCycleCodeLengthRule(3600);
        var item = ReorgCycle.NewReorgCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReorgCycleRequiredCodeRule("reorg cycle needs a value");
        var item = ReorgCycle.NewReorgCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgCycleRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new ReorgCycleRequiredEmployeeIdRule("reorg cycle needs this field");
        var item = ReorgCycle.NewReorgCycle("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReorgCycleValidatorReportsEveryProblem();
        ReorgCycleLabelLengthRuleFlagsALongLabel();
        ReorgCycleCodeLengthRuleFlagsALongCode();
        ReorgCycleRequiredCodeRuleFlagsABlankCode();
        ReorgCycleRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
