namespace Acme.Workforce.Absence.Tests.Accrual.Tier;

using Acme.Workforce.Absence.Accrual.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccrualTierChecks
{
    public static void AccrualTierStartsInNew()
    {
        var item = AccrualTier.NewAccrualTier("code-140", "label-970");
        Check.Equal(item.Status, AccrualTierStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualTierMovesToDone()
    {
        var item = AccrualTier.NewAccrualTier("code-140", "label-970");
        try
        {
            item.MoveTo(AccrualTierStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualTierStatus.Done, "status");
    }

    public static void AccrualTierRejectsMovesFromFailed()
    {
        var item = AccrualTier.NewAccrualTier("code-140", "label-970");
        item.Status = AccrualTierStatus.Failed;
        Check.Fails(() => item.MoveTo(AccrualTierStatus.New), "move");
    }

    public static void RunAll()
    {
        AccrualTierStartsInNew();
        AccrualTierMovesToDone();
        AccrualTierRejectsMovesFromFailed();
    }
}
