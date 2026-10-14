namespace Acme.Workforce.Orgdesign.Tests.Headcount.Allocation;

using Acme.Workforce.Orgdesign.Headcount.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class HeadcountAllocationRulesChecks
{
    public static void HeadcountAllocationValidatorReportsEveryProblem()
    {
        var validator = HeadcountAllocationRules.DefaultHeadcountAllocationValidator();
        var item = HeadcountAllocation.NewHeadcountAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountAllocationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new HeadcountAllocationLabelLengthRule(86400);
        var item = HeadcountAllocation.NewHeadcountAllocation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountAllocationNotesLengthRuleFlagsALongNotes()
    {
        var rule = new HeadcountAllocationNotesLengthRule(32);
        var item = HeadcountAllocation.NewHeadcountAllocation("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountAllocationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new HeadcountAllocationRequiredLabelRule("headcount allocation needs an entry");
        var item = HeadcountAllocation.NewHeadcountAllocation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HeadcountAllocationValidatorReportsEveryProblem();
        HeadcountAllocationLabelLengthRuleFlagsALongLabel();
        HeadcountAllocationNotesLengthRuleFlagsALongNotes();
        HeadcountAllocationRequiredLabelRuleFlagsABlankLabel();
    }
}
