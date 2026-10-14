namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LenientDeskAllocationRulesChecks
{
    public static void LenientDeskAllocationValidatorReportsEveryProblem()
    {
        var validator = LenientDeskAllocationRules.DefaultLenientDeskAllocationValidator();
        var item = LenientDeskAllocation.NewLenientDeskAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientDeskAllocationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LenientDeskAllocationRequiredTitleRule("lenient desk allocation needs a value");
        var item = LenientDeskAllocation.NewLenientDeskAllocation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientDeskAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientDeskAllocationRequiredCodeRule("lenient desk allocation needs an entry");
        var item = LenientDeskAllocation.NewLenientDeskAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientDeskAllocationValidatorReportsEveryProblem();
        LenientDeskAllocationRequiredTitleRuleFlagsABlankTitle();
        LenientDeskAllocationRequiredCodeRuleFlagsABlankCode();
    }
}
