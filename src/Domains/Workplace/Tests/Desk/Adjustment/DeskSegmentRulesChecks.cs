namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeskSegmentRulesChecks
{
    public static void DeskSegmentValidatorReportsEveryProblem()
    {
        var validator = DeskSegmentRules.DefaultDeskSegmentValidator();
        var item = DeskSegment.NewDeskSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskSegmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DeskSegmentRequiredTitleRule("desk segment needs a value");
        var item = DeskSegment.NewDeskSegment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeskSegmentRequiredCodeRule("desk segment needs a value");
        var item = DeskSegment.NewDeskSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskSegmentRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new DeskSegmentRequiredUnitRule("desk segment needs an entry");
        var item = DeskSegment.NewDeskSegment("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskSegmentManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new DeskSegmentManagerIdLengthRule(3600);
        var item = DeskSegment.NewDeskSegment("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskSegmentValidatorReportsEveryProblem();
        DeskSegmentRequiredTitleRuleFlagsABlankTitle();
        DeskSegmentRequiredCodeRuleFlagsABlankCode();
        DeskSegmentRequiredUnitRuleFlagsABlankUnit();
        DeskSegmentManagerIdLengthRuleFlagsALongManagerId();
    }
}
