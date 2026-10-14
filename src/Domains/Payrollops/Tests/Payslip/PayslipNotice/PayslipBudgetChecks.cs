namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class PayslipBudgetChecks
{
    public static void PayslipBudgetStartsInRequested()
    {
        var item = PayslipBudget.NewPayslipBudget("code-872", "title-911");
        Check.Equal(item.Status, PayslipBudgetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipBudgetMovesToRevoked()
    {
        var item = PayslipBudget.NewPayslipBudget("code-872", "title-911");
        try
        {
            item.MoveTo(PayslipBudgetStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipBudgetStatus.Revoked, "status");
    }

    public static void PayslipBudgetRejectsMovesFromExpired()
    {
        var item = PayslipBudget.NewPayslipBudget("code-872", "title-911");
        item.Status = PayslipBudgetStatus.Expired;
        Check.Fails(() => item.MoveTo(PayslipBudgetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayslipBudgetStartsInRequested();
        PayslipBudgetMovesToRevoked();
        PayslipBudgetRejectsMovesFromExpired();
    }
}
