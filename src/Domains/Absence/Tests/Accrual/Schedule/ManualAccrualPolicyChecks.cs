namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ManualAccrualPolicyChecks
{
    public static void ManualAccrualPolicyStartsInCreated()
    {
        var item = ManualAccrualPolicy.NewManualAccrualPolicy("code-193", "name-557");
        Check.Equal(item.Status, ManualAccrualPolicyStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualAccrualPolicyMovesToPublished()
    {
        var item = ManualAccrualPolicy.NewManualAccrualPolicy("code-193", "name-557");
        try
        {
            item.MoveTo(ManualAccrualPolicyStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualAccrualPolicyStatus.Published, "status");
    }

    public static void ManualAccrualPolicyRejectsMovesFromRetired()
    {
        var item = ManualAccrualPolicy.NewManualAccrualPolicy("code-193", "name-557");
        item.Status = ManualAccrualPolicyStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualAccrualPolicyStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualAccrualPolicyStartsInCreated();
        ManualAccrualPolicyMovesToPublished();
        ManualAccrualPolicyRejectsMovesFromRetired();
    }
}
