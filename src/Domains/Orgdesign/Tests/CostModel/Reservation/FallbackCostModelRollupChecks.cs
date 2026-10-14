namespace Acme.Workforce.Orgdesign.Tests.CostModel.Reservation;

using Acme.Workforce.Orgdesign.CostModel.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FallbackCostModelRollupChecks
{
    public static void FallbackCostModelRollupStartsInCreated()
    {
        var item = FallbackCostModelRollup.NewFallbackCostModelRollup("code-649", "name-914");
        Check.Equal(item.Status, FallbackCostModelRollupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackCostModelRollupMovesToPublished()
    {
        var item = FallbackCostModelRollup.NewFallbackCostModelRollup("code-649", "name-914");
        try
        {
            item.MoveTo(FallbackCostModelRollupStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackCostModelRollupStatus.Published, "status");
    }

    public static void FallbackCostModelRollupRejectsMovesFromRetired()
    {
        var item = FallbackCostModelRollup.NewFallbackCostModelRollup("code-649", "name-914");
        item.Status = FallbackCostModelRollupStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackCostModelRollupStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackCostModelRollupStartsInCreated();
        FallbackCostModelRollupMovesToPublished();
        FallbackCostModelRollupRejectsMovesFromRetired();
    }
}
