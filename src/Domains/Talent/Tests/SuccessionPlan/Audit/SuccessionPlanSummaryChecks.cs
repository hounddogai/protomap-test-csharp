namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SuccessionPlanSummaryChecks
{
    public static void SuccessionPlanSummaryStartsInRequested()
    {
        var item = SuccessionPlanSummary.NewSuccessionPlanSummary("code-182", "label-366");
        Check.Equal(item.Status, SuccessionPlanSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanSummaryMovesToRevoked()
    {
        var item = SuccessionPlanSummary.NewSuccessionPlanSummary("code-182", "label-366");
        try
        {
            item.MoveTo(SuccessionPlanSummaryStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanSummaryStatus.Revoked, "status");
    }

    public static void SuccessionPlanSummaryRejectsMovesFromExpired()
    {
        var item = SuccessionPlanSummary.NewSuccessionPlanSummary("code-182", "label-366");
        item.Status = SuccessionPlanSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(SuccessionPlanSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanSummaryStartsInRequested();
        SuccessionPlanSummaryMovesToRevoked();
        SuccessionPlanSummaryRejectsMovesFromExpired();
    }
}
