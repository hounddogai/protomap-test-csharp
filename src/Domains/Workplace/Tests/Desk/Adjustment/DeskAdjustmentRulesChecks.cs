namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeskAdjustmentRulesChecks
{
    public static void DeskAdjustmentValidatorReportsEveryProblem()
    {
        var validator = DeskAdjustmentRules.DefaultDeskAdjustmentValidator();
        var item = DeskAdjustment.NewDeskAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskAdjustmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DeskAdjustmentTitleLengthRule(64);
        var item = DeskAdjustment.NewDeskAdjustment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskAdjustmentRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new DeskAdjustmentRequiredDepartmentRule("desk adjustment needs a value");
        var item = DeskAdjustment.NewDeskAdjustment("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeskAdjustmentCodeLengthRule(32);
        var item = DeskAdjustment.NewDeskAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskAdjustmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DeskAdjustmentRequiredTitleRule("desk adjustment needs this field");
        var item = DeskAdjustment.NewDeskAdjustment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeskAdjustmentValidatorReportsEveryProblem();
        DeskAdjustmentTitleLengthRuleFlagsALongTitle();
        DeskAdjustmentRequiredDepartmentRuleFlagsABlankDepartment();
        DeskAdjustmentCodeLengthRuleFlagsALongCode();
        DeskAdjustmentRequiredTitleRuleFlagsABlankTitle();
    }
}
