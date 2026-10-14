namespace Acme.Workforce.Compensation.Tests.MeritCycle.Request;

using Acme.Workforce.Compensation.MeritCycle.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class MeritCycleRequestChecks
{
    public static void MeritCycleRequestStartsInCreated()
    {
        var item = MeritCycleRequest.NewMeritCycleRequest("code-944", "name-80");
        Check.Equal(item.Status, MeritCycleRequestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleRequestMovesToRetired()
    {
        var item = MeritCycleRequest.NewMeritCycleRequest("code-944", "name-80");
        try
        {
            item.MoveTo(MeritCycleRequestStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleRequestStatus.Retired, "status");
    }

    public static void MeritCycleRequestRejectsMovesFromRetired()
    {
        var item = MeritCycleRequest.NewMeritCycleRequest("code-944", "name-80");
        item.Status = MeritCycleRequestStatus.Retired;
        Check.Fails(() => item.MoveTo(MeritCycleRequestStatus.Created), "move");
    }

    public static void RunAll()
    {
        MeritCycleRequestStartsInCreated();
        MeritCycleRequestMovesToRetired();
        MeritCycleRequestRejectsMovesFromRetired();
    }
}
