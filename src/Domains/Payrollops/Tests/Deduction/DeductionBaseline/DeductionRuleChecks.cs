namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBaseline;

using Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

/// <summary>Checks of deduction baseline code, run by the project's runner.</summary>
public static class DeductionRuleChecks
{
    public static void DeductionRuleStartsInIdle()
    {
        var item = DeductionRule.NewDeductionRule("code-436", "name-264");
        Check.Equal(item.Status, DeductionRuleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionRuleMovesToWarming()
    {
        var item = DeductionRule.NewDeductionRule("code-436", "name-264");
        try
        {
            item.MoveTo(DeductionRuleStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionRuleStatus.Warming, "status");
    }

    public static void DeductionRuleRejectsMovesFromStopped()
    {
        var item = DeductionRule.NewDeductionRule("code-436", "name-264");
        item.Status = DeductionRuleStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeductionRuleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeductionRuleStartsInIdle();
        DeductionRuleMovesToWarming();
        DeductionRuleRejectsMovesFromStopped();
    }
}
