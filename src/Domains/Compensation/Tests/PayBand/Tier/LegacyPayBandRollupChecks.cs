namespace Acme.Workforce.Compensation.Tests.PayBand.Tier;

using Acme.Workforce.Compensation.PayBand.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LegacyPayBandRollupChecks
{
    public static void LegacyPayBandRollupStartsInDraft()
    {
        var item = LegacyPayBandRollup.NewLegacyPayBandRollup("code-311", "name-144");
        Check.Equal(item.Status, LegacyPayBandRollupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPayBandRollupMovesToActive()
    {
        var item = LegacyPayBandRollup.NewLegacyPayBandRollup("code-311", "name-144");
        try
        {
            item.MoveTo(LegacyPayBandRollupStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPayBandRollupStatus.Active, "status");
    }

    public static void LegacyPayBandRollupRejectsMovesFromClosed()
    {
        var item = LegacyPayBandRollup.NewLegacyPayBandRollup("code-311", "name-144");
        item.Status = LegacyPayBandRollupStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyPayBandRollupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyPayBandRollupStartsInDraft();
        LegacyPayBandRollupMovesToActive();
        LegacyPayBandRollupRejectsMovesFromClosed();
    }
}
