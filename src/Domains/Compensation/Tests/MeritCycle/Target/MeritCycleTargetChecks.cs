namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleTargetChecks
{
    public static void MeritCycleTargetStartsInRequested()
    {
        var item = MeritCycleTarget.NewMeritCycleTarget("code-101", "name-67");
        Check.Equal(item.Status, MeritCycleTargetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleTargetMovesToRevoked()
    {
        var item = MeritCycleTarget.NewMeritCycleTarget("code-101", "name-67");
        try
        {
            item.MoveTo(MeritCycleTargetStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleTargetStatus.Revoked, "status");
    }

    public static void MeritCycleTargetRejectsMovesFromExpired()
    {
        var item = MeritCycleTarget.NewMeritCycleTarget("code-101", "name-67");
        item.Status = MeritCycleTargetStatus.Expired;
        Check.Fails(() => item.MoveTo(MeritCycleTargetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        MeritCycleTargetStartsInRequested();
        MeritCycleTargetMovesToRevoked();
        MeritCycleTargetRejectsMovesFromExpired();
    }
}
