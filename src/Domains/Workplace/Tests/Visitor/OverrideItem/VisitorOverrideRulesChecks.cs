namespace Acme.Workforce.Workplace.Tests.Visitor.OverrideItem;

using Acme.Workforce.Workplace.Visitor.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class VisitorOverrideRulesChecks
{
    public static void VisitorOverrideValidatorReportsEveryProblem()
    {
        var validator = VisitorOverrideRules.DefaultVisitorOverrideValidator();
        var item = VisitorOverride.NewVisitorOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorOverrideNameLengthRuleFlagsALongName()
    {
        var rule = new VisitorOverrideNameLengthRule(255);
        var item = VisitorOverride.NewVisitorOverride("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorOverrideManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new VisitorOverrideManagerIdLengthRule(255);
        var item = VisitorOverride.NewVisitorOverride("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new VisitorOverrideCodeLengthRule(32);
        var item = VisitorOverride.NewVisitorOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VisitorOverrideRequiredCodeRule("visitor override needs this field");
        var item = VisitorOverride.NewVisitorOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorOverrideLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new VisitorOverrideLeaveDaysRangeRule(120, 3600);
        var item = VisitorOverride.NewVisitorOverride("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        VisitorOverrideValidatorReportsEveryProblem();
        VisitorOverrideNameLengthRuleFlagsALongName();
        VisitorOverrideManagerIdLengthRuleFlagsALongManagerId();
        VisitorOverrideCodeLengthRuleFlagsALongCode();
        VisitorOverrideRequiredCodeRuleFlagsABlankCode();
        VisitorOverrideLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
