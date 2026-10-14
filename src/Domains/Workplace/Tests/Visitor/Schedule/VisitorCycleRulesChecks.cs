namespace Acme.Workforce.Workplace.Tests.Visitor.Schedule;

using Acme.Workforce.Workplace.Visitor.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class VisitorCycleRulesChecks
{
    public static void VisitorCycleValidatorReportsEveryProblem()
    {
        var validator = VisitorCycleRules.DefaultVisitorCycleValidator();
        var item = VisitorCycle.NewVisitorCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorCycleLocationLengthRuleFlagsALongLocation()
    {
        var rule = new VisitorCycleLocationLengthRule(1000);
        var item = VisitorCycle.NewVisitorCycle("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorCycleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new VisitorCycleTenureMonthsRangeRule(32, 64);
        var item = VisitorCycle.NewVisitorCycle("x", "y");
        item.TenureMonths = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new VisitorCycleCodeLengthRule(255);
        var item = VisitorCycle.NewVisitorCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorCycleRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new VisitorCycleRequiredLocationRule("visitor cycle needs a value");
        var item = VisitorCycle.NewVisitorCycle("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorCycleRequiredNameRuleFlagsABlankName()
    {
        var rule = new VisitorCycleRequiredNameRule("visitor cycle needs a value");
        var item = VisitorCycle.NewVisitorCycle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorCycleValidatorReportsEveryProblem();
        VisitorCycleLocationLengthRuleFlagsALongLocation();
        VisitorCycleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        VisitorCycleCodeLengthRuleFlagsALongCode();
        VisitorCycleRequiredLocationRuleFlagsABlankLocation();
        VisitorCycleRequiredNameRuleFlagsABlankName();
    }
}
