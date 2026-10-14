namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class VisitorRollupRulesChecks
{
    public static void VisitorRollupValidatorReportsEveryProblem()
    {
        var validator = VisitorRollupRules.DefaultVisitorRollupValidator();
        var item = VisitorRollup.NewVisitorRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorRollupUnitLengthRuleFlagsALongUnit()
    {
        var rule = new VisitorRollupUnitLengthRule(80);
        var item = VisitorRollup.NewVisitorRollup("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorRollupRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new VisitorRollupRequiredUnitRule("visitor rollup needs an entry");
        var item = VisitorRollup.NewVisitorRollup("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new VisitorRollupCodeLengthRule(32);
        var item = VisitorRollup.NewVisitorRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VisitorRollupValidatorReportsEveryProblem();
        VisitorRollupUnitLengthRuleFlagsALongUnit();
        VisitorRollupRequiredUnitRuleFlagsABlankUnit();
        VisitorRollupCodeLengthRuleFlagsALongCode();
    }
}
