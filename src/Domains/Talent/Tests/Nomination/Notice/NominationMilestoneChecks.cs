namespace Acme.Workforce.Talent.Tests.Nomination.Notice;

using Acme.Workforce.Talent.Nomination.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class NominationMilestoneChecks
{
    public static void NominationMilestoneStartsInCreated()
    {
        var item = NominationMilestone.NewNominationMilestone("code-82", "title-722");
        Check.Equal(item.Status, NominationMilestoneStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationMilestoneMovesToRetired()
    {
        var item = NominationMilestone.NewNominationMilestone("code-82", "title-722");
        try
        {
            item.MoveTo(NominationMilestoneStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationMilestoneStatus.Retired, "status");
    }

    public static void NominationMilestoneRejectsMovesFromRetired()
    {
        var item = NominationMilestone.NewNominationMilestone("code-82", "title-722");
        item.Status = NominationMilestoneStatus.Retired;
        Check.Fails(() => item.MoveTo(NominationMilestoneStatus.Created), "move");
    }

    public static void RunAll()
    {
        NominationMilestoneStartsInCreated();
        NominationMilestoneMovesToRetired();
        NominationMilestoneRejectsMovesFromRetired();
    }
}
