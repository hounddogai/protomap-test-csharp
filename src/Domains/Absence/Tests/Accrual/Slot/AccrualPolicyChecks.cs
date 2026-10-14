namespace Acme.Workforce.Absence.Tests.Accrual.Slot;

using Acme.Workforce.Absence.Accrual.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AccrualPolicyChecks
{
    public static void AccrualPolicyStartsInCreated()
    {
        var item = AccrualPolicy.NewAccrualPolicy("code-958", "label-944");
        Check.Equal(item.Status, AccrualPolicyStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualPolicyMovesToRetired()
    {
        var item = AccrualPolicy.NewAccrualPolicy("code-958", "label-944");
        try
        {
            item.MoveTo(AccrualPolicyStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualPolicyStatus.Retired, "status");
    }

    public static void AccrualPolicyRejectsMovesFromRetired()
    {
        var item = AccrualPolicy.NewAccrualPolicy("code-958", "label-944");
        item.Status = AccrualPolicyStatus.Retired;
        Check.Fails(() => item.MoveTo(AccrualPolicyStatus.Created), "move");
    }

    public static void RunAll()
    {
        AccrualPolicyStartsInCreated();
        AccrualPolicyMovesToRetired();
        AccrualPolicyRejectsMovesFromRetired();
    }
}
