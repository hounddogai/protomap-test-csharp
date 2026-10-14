namespace Acme.Workforce.Payrollops.Tests.Deduction.Metric;

using Acme.Workforce.Payrollops.Deduction.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedDeductionPolicyChecks
{
    public static void CappedDeductionPolicyStartsInPlanned()
    {
        var item = CappedDeductionPolicy.NewCappedDeductionPolicy("code-743", "name-96");
        Check.Equal(item.Status, CappedDeductionPolicyStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedDeductionPolicyMovesToCompleted()
    {
        var item = CappedDeductionPolicy.NewCappedDeductionPolicy("code-743", "name-96");
        try
        {
            item.MoveTo(CappedDeductionPolicyStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedDeductionPolicyStatus.Completed, "status");
    }

    public static void CappedDeductionPolicyRejectsMovesFromCancelled()
    {
        var item = CappedDeductionPolicy.NewCappedDeductionPolicy("code-743", "name-96");
        item.Status = CappedDeductionPolicyStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CappedDeductionPolicyStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CappedDeductionPolicyStartsInPlanned();
        CappedDeductionPolicyMovesToCompleted();
        CappedDeductionPolicyRejectsMovesFromCancelled();
    }
}
