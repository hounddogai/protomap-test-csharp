namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScheduledMobilityRequestChecks
{
    public static void ScheduledMobilityRequestStartsInDraft()
    {
        var item = ScheduledMobilityRequest.NewScheduledMobilityRequest("code-289", "name-650");
        Check.Equal(item.Status, ScheduledMobilityRequestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledMobilityRequestMovesToClosed()
    {
        var item = ScheduledMobilityRequest.NewScheduledMobilityRequest("code-289", "name-650");
        try
        {
            item.MoveTo(ScheduledMobilityRequestStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledMobilityRequestStatus.Closed, "status");
    }

    public static void ScheduledMobilityRequestRejectsMovesFromClosed()
    {
        var item = ScheduledMobilityRequest.NewScheduledMobilityRequest("code-289", "name-650");
        item.Status = ScheduledMobilityRequestStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledMobilityRequestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledMobilityRequestStartsInDraft();
        ScheduledMobilityRequestMovesToClosed();
        ScheduledMobilityRequestRejectsMovesFromClosed();
    }
}
