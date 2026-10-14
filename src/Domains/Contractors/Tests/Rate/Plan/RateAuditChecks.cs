namespace Acme.Workforce.Contractors.Tests.Rate.Plan;

using Acme.Workforce.Contractors.Rate.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RateAuditChecks
{
    public static void RateAuditStartsInPlanned()
    {
        var item = RateAudit.NewRateAudit("code-929", "name-968");
        Check.Equal(item.Status, RateAuditStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateAuditMovesToCancelled()
    {
        var item = RateAudit.NewRateAudit("code-929", "name-968");
        try
        {
            item.MoveTo(RateAuditStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateAuditStatus.Cancelled, "status");
    }

    public static void RateAuditRejectsMovesFromCancelled()
    {
        var item = RateAudit.NewRateAudit("code-929", "name-968");
        item.Status = RateAuditStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RateAuditStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RateAuditStartsInPlanned();
        RateAuditMovesToCancelled();
        RateAuditRejectsMovesFromCancelled();
    }
}
