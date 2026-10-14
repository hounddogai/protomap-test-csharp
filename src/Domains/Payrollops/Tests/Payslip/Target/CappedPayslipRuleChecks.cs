namespace Acme.Workforce.Payrollops.Tests.Payslip.Target;

using Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class CappedPayslipRuleChecks
{
    public static void CappedPayslipRuleStartsInDraft()
    {
        var item = CappedPayslipRule.NewCappedPayslipRule("code-44", "title-484");
        Check.Equal(item.Status, CappedPayslipRuleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPayslipRuleMovesToActive()
    {
        var item = CappedPayslipRule.NewCappedPayslipRule("code-44", "title-484");
        try
        {
            item.MoveTo(CappedPayslipRuleStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPayslipRuleStatus.Active, "status");
    }

    public static void CappedPayslipRuleRejectsMovesFromClosed()
    {
        var item = CappedPayslipRule.NewCappedPayslipRule("code-44", "title-484");
        item.Status = CappedPayslipRuleStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedPayslipRuleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedPayslipRuleStartsInDraft();
        CappedPayslipRuleMovesToActive();
        CappedPayslipRuleRejectsMovesFromClosed();
    }
}
