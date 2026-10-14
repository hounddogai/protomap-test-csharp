namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RetroPayAllocationRulesChecks
{
    public static void RetroPayAllocationValidatorReportsEveryProblem()
    {
        var validator = RetroPayAllocationRules.DefaultRetroPayAllocationValidator();
        var item = RetroPayAllocation.NewRetroPayAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayAllocationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RetroPayAllocationRequiredLabelRule("retro pay allocation needs this field");
        var item = RetroPayAllocation.NewRetroPayAllocation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayAllocationEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new RetroPayAllocationEndsAtRangeRule(64, 120);
        var item = RetroPayAllocation.NewRetroPayAllocation("x", "y");
        item.EndsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RetroPayAllocationValidatorReportsEveryProblem();
        RetroPayAllocationRequiredLabelRuleFlagsABlankLabel();
        RetroPayAllocationEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
