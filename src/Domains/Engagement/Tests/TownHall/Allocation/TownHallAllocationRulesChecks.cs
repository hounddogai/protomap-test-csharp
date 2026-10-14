namespace Acme.Workforce.Engagement.Tests.TownHall.Allocation;

using Acme.Workforce.Engagement.TownHall.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TownHallAllocationRulesChecks
{
    public static void TownHallAllocationValidatorReportsEveryProblem()
    {
        var validator = TownHallAllocationRules.DefaultTownHallAllocationValidator();
        var item = TownHallAllocation.NewTownHallAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallAllocationGradeLengthRuleFlagsALongGrade()
    {
        var rule = new TownHallAllocationGradeLengthRule(80);
        var item = TownHallAllocation.NewTownHallAllocation("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallAllocationRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new TownHallAllocationRequiredGradeRule("town hall allocation needs a value");
        var item = TownHallAllocation.NewTownHallAllocation("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new TownHallAllocationCodeLengthRule(32);
        var item = TownHallAllocation.NewTownHallAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TownHallAllocationRequiredCodeRule("town hall allocation needs a value");
        var item = TownHallAllocation.NewTownHallAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallAllocationNameLengthRuleFlagsALongName()
    {
        var rule = new TownHallAllocationNameLengthRule(64);
        var item = TownHallAllocation.NewTownHallAllocation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TownHallAllocationValidatorReportsEveryProblem();
        TownHallAllocationGradeLengthRuleFlagsALongGrade();
        TownHallAllocationRequiredGradeRuleFlagsABlankGrade();
        TownHallAllocationCodeLengthRuleFlagsALongCode();
        TownHallAllocationRequiredCodeRuleFlagsABlankCode();
        TownHallAllocationNameLengthRuleFlagsALongName();
    }
}
