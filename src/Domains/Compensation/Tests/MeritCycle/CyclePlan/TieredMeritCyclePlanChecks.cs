namespace Acme.Workforce.Compensation.Tests.MeritCycle.CyclePlan;

using Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>Checks of cycle plan code, run by the project's runner.</summary>
public static class TieredMeritCyclePlanChecks
{
    public static void TieredMeritCyclePlanStartsInCreated()
    {
        var item = TieredMeritCyclePlan.NewTieredMeritCyclePlan("code-367", "name-402");
        Check.Equal(item.Status, TieredMeritCyclePlanStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredMeritCyclePlanMovesToValidated()
    {
        var item = TieredMeritCyclePlan.NewTieredMeritCyclePlan("code-367", "name-402");
        try
        {
            item.MoveTo(TieredMeritCyclePlanStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredMeritCyclePlanStatus.Validated, "status");
    }

    public static void TieredMeritCyclePlanRejectsMovesFromRetired()
    {
        var item = TieredMeritCyclePlan.NewTieredMeritCyclePlan("code-367", "name-402");
        item.Status = TieredMeritCyclePlanStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredMeritCyclePlanStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredMeritCyclePlanStartsInCreated();
        TieredMeritCyclePlanMovesToValidated();
        TieredMeritCyclePlanRejectsMovesFromRetired();
    }
}
