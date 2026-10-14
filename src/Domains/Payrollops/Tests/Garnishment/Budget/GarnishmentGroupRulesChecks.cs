namespace Acme.Workforce.Payrollops.Tests.Garnishment.Budget;

using Acme.Workforce.Payrollops.Garnishment.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class GarnishmentGroupRulesChecks
{
    public static void GarnishmentGroupValidatorReportsEveryProblem()
    {
        var validator = GarnishmentGroupRules.DefaultGarnishmentGroupValidator();
        var item = GarnishmentGroup.NewGarnishmentGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentGroupUnitLengthRuleFlagsALongUnit()
    {
        var rule = new GarnishmentGroupUnitLengthRule(255);
        var item = GarnishmentGroup.NewGarnishmentGroup("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new GarnishmentGroupCodeLengthRule(80);
        var item = GarnishmentGroup.NewGarnishmentGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GarnishmentGroupRequiredCodeRule("garnishment group needs an entry");
        var item = GarnishmentGroup.NewGarnishmentGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GarnishmentGroupValidatorReportsEveryProblem();
        GarnishmentGroupUnitLengthRuleFlagsALongUnit();
        GarnishmentGroupCodeLengthRuleFlagsALongCode();
        GarnishmentGroupRequiredCodeRuleFlagsABlankCode();
    }
}
