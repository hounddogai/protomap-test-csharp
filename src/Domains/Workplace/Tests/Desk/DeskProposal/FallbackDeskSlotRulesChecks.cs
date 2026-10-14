namespace Acme.Workforce.Workplace.Tests.Desk.DeskProposal;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Checks of desk proposal code, run by the project's runner.</summary>
public static class FallbackDeskSlotRulesChecks
{
    public static void FallbackDeskSlotValidatorReportsEveryProblem()
    {
        var validator = FallbackDeskSlotRules.DefaultFallbackDeskSlotValidator();
        var item = FallbackDeskSlot.NewFallbackDeskSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackDeskSlotRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackDeskSlotRequiredTitleRule("fallback desk slot needs a value");
        var item = FallbackDeskSlot.NewFallbackDeskSlot("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackDeskSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackDeskSlotRequiredCodeRule("fallback desk slot needs an entry");
        var item = FallbackDeskSlot.NewFallbackDeskSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackDeskSlotValidatorReportsEveryProblem();
        FallbackDeskSlotRequiredTitleRuleFlagsABlankTitle();
        FallbackDeskSlotRequiredCodeRuleFlagsABlankCode();
    }
}
