namespace Acme.Workforce.Compensation.Tests.MeritCycle.CyclePlan;

using Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>Checks of cycle plan code, run by the project's runner.</summary>
public static class MeritCycleSummaryChecks
{
    public static void MeritCycleSummaryStartsInProposed()
    {
        var item = MeritCycleSummary.NewMeritCycleSummary("code-839", "label-237");
        Check.Equal(item.Status, MeritCycleSummaryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleSummaryMovesToApproved()
    {
        var item = MeritCycleSummary.NewMeritCycleSummary("code-839", "label-237");
        try
        {
            item.MoveTo(MeritCycleSummaryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleSummaryStatus.Approved, "status");
    }

    public static void MeritCycleSummaryRejectsMovesFromWithdrawn()
    {
        var item = MeritCycleSummary.NewMeritCycleSummary("code-839", "label-237");
        item.Status = MeritCycleSummaryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(MeritCycleSummaryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        MeritCycleSummaryStartsInProposed();
        MeritCycleSummaryMovesToApproved();
        MeritCycleSummaryRejectsMovesFromWithdrawn();
    }
}
