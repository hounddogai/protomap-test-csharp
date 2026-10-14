namespace Acme.Workforce.Compensation.Tests.MeritCycle.Tier;

using Acme.Workforce.Compensation.MeritCycle.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MeritCycleTierChecks
{
    public static void MeritCycleTierStartsInCreated()
    {
        var item = MeritCycleTier.NewMeritCycleTier("code-456", "title-852");
        Check.Equal(item.Status, MeritCycleTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleTierMovesToRetired()
    {
        var item = MeritCycleTier.NewMeritCycleTier("code-456", "title-852");
        try
        {
            item.MoveTo(MeritCycleTierStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleTierStatus.Retired, "status");
    }

    public static void MeritCycleTierRejectsMovesFromRetired()
    {
        var item = MeritCycleTier.NewMeritCycleTier("code-456", "title-852");
        item.Status = MeritCycleTierStatus.Retired;
        Check.Fails(() => item.MoveTo(MeritCycleTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        MeritCycleTierStartsInCreated();
        MeritCycleTierMovesToRetired();
        MeritCycleTierRejectsMovesFromRetired();
    }
}
