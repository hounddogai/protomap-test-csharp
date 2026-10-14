namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AccrualBundleChecks
{
    public static void AccrualBundleStartsInNew()
    {
        var item = AccrualBundle.NewAccrualBundle("code-858", "name-894");
        Check.Equal(item.Status, AccrualBundleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualBundleMovesToQueued()
    {
        var item = AccrualBundle.NewAccrualBundle("code-858", "name-894");
        try
        {
            item.MoveTo(AccrualBundleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualBundleStatus.Queued, "status");
    }

    public static void AccrualBundleRejectsMovesFromFailed()
    {
        var item = AccrualBundle.NewAccrualBundle("code-858", "name-894");
        item.Status = AccrualBundleStatus.Failed;
        Check.Fails(() => item.MoveTo(AccrualBundleStatus.New), "move");
    }

    public static void RunAll()
    {
        AccrualBundleStartsInNew();
        AccrualBundleMovesToQueued();
        AccrualBundleRejectsMovesFromFailed();
    }
}
