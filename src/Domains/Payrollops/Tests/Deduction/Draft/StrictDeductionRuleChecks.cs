namespace Acme.Workforce.Payrollops.Tests.Deduction.Draft;

using Acme.Workforce.Payrollops.Deduction.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class StrictDeductionRuleChecks
{
    public static void StrictDeductionRuleStartsInRequested()
    {
        var item = StrictDeductionRule.NewStrictDeductionRule("code-674", "label-252");
        Check.Equal(item.Status, StrictDeductionRuleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictDeductionRuleMovesToGranted()
    {
        var item = StrictDeductionRule.NewStrictDeductionRule("code-674", "label-252");
        try
        {
            item.MoveTo(StrictDeductionRuleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictDeductionRuleStatus.Granted, "status");
    }

    public static void StrictDeductionRuleRejectsMovesFromExpired()
    {
        var item = StrictDeductionRule.NewStrictDeductionRule("code-674", "label-252");
        item.Status = StrictDeductionRuleStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictDeductionRuleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictDeductionRuleStartsInRequested();
        StrictDeductionRuleMovesToGranted();
        StrictDeductionRuleRejectsMovesFromExpired();
    }
}
