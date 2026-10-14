namespace Acme.Workforce.Payrollops.Tests.Deduction.Policy;

using Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class GlobalDeductionPolicyChecks
{
    public static void GlobalDeductionPolicyStartsInNew()
    {
        var item = GlobalDeductionPolicy.NewGlobalDeductionPolicy("code-979", "name-114");
        Check.Equal(item.Status, GlobalDeductionPolicyStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalDeductionPolicyMovesToDone()
    {
        var item = GlobalDeductionPolicy.NewGlobalDeductionPolicy("code-979", "name-114");
        try
        {
            item.MoveTo(GlobalDeductionPolicyStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalDeductionPolicyStatus.Done, "status");
    }

    public static void GlobalDeductionPolicyRejectsMovesFromFailed()
    {
        var item = GlobalDeductionPolicy.NewGlobalDeductionPolicy("code-979", "name-114");
        item.Status = GlobalDeductionPolicyStatus.Failed;
        Check.Fails(() => item.MoveTo(GlobalDeductionPolicyStatus.New), "move");
    }

    public static void RunAll()
    {
        GlobalDeductionPolicyStartsInNew();
        GlobalDeductionPolicyMovesToDone();
        GlobalDeductionPolicyRejectsMovesFromFailed();
    }
}
