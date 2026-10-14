namespace Acme.Workforce.Talent.Tests.Nomination.Cycle;

using Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RollingNominationThresholdChecks
{
    public static void RollingNominationThresholdStartsInCreated()
    {
        var item = RollingNominationThreshold.NewRollingNominationThreshold("code-765", "title-743");
        Check.Equal(item.Status, RollingNominationThresholdStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingNominationThresholdMovesToPublished()
    {
        var item = RollingNominationThreshold.NewRollingNominationThreshold("code-765", "title-743");
        try
        {
            item.MoveTo(RollingNominationThresholdStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingNominationThresholdStatus.Published, "status");
    }

    public static void RollingNominationThresholdRejectsMovesFromRetired()
    {
        var item = RollingNominationThreshold.NewRollingNominationThreshold("code-765", "title-743");
        item.Status = RollingNominationThresholdStatus.Retired;
        Check.Fails(() => item.MoveTo(RollingNominationThresholdStatus.Created), "move");
    }

    public static void RunAll()
    {
        RollingNominationThresholdStartsInCreated();
        RollingNominationThresholdMovesToPublished();
        RollingNominationThresholdRejectsMovesFromRetired();
    }
}
