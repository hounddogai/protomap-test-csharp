namespace Acme.Workforce.Absence.Tests.Accrual.Rollup;

using Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryAccrualTierChecks
{
    public static void PrimaryAccrualTierStartsInRequested()
    {
        var item = PrimaryAccrualTier.NewPrimaryAccrualTier("code-770", "name-268");
        Check.Equal(item.Status, PrimaryAccrualTierStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryAccrualTierMovesToRevoked()
    {
        var item = PrimaryAccrualTier.NewPrimaryAccrualTier("code-770", "name-268");
        try
        {
            item.MoveTo(PrimaryAccrualTierStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryAccrualTierStatus.Revoked, "status");
    }

    public static void PrimaryAccrualTierRejectsMovesFromExpired()
    {
        var item = PrimaryAccrualTier.NewPrimaryAccrualTier("code-770", "name-268");
        item.Status = PrimaryAccrualTierStatus.Expired;
        Check.Fails(() => item.MoveTo(PrimaryAccrualTierStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PrimaryAccrualTierStartsInRequested();
        PrimaryAccrualTierMovesToRevoked();
        PrimaryAccrualTierRejectsMovesFromExpired();
    }
}
