namespace Acme.Workforce.Talent.Tests.Nomination.Group;

using Acme.Workforce.Talent.Nomination.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class NominationPlanChecks
{
    public static void NominationPlanStartsInPlanned()
    {
        var item = NominationPlan.NewNominationPlan("code-127", "label-252");
        Check.Equal(item.Status, NominationPlanStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationPlanMovesToCompleted()
    {
        var item = NominationPlan.NewNominationPlan("code-127", "label-252");
        try
        {
            item.MoveTo(NominationPlanStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationPlanStatus.Completed, "status");
    }

    public static void NominationPlanRejectsMovesFromCancelled()
    {
        var item = NominationPlan.NewNominationPlan("code-127", "label-252");
        item.Status = NominationPlanStatus.Cancelled;
        Check.Fails(() => item.MoveTo(NominationPlanStatus.Planned), "move");
    }

    public static void RunAll()
    {
        NominationPlanStartsInPlanned();
        NominationPlanMovesToCompleted();
        NominationPlanRejectsMovesFromCancelled();
    }
}
