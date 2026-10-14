namespace Acme.Workforce.Payrollops.Tests.Garnishment.Target;

using Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class GarnishmentSegmentChecks
{
    public static void GarnishmentSegmentStartsInProposed()
    {
        var item = GarnishmentSegment.NewGarnishmentSegment("code-56", "label-94");
        Check.Equal(item.Status, GarnishmentSegmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentSegmentMovesToApproved()
    {
        var item = GarnishmentSegment.NewGarnishmentSegment("code-56", "label-94");
        try
        {
            item.MoveTo(GarnishmentSegmentStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentSegmentStatus.Approved, "status");
    }

    public static void GarnishmentSegmentRejectsMovesFromWithdrawn()
    {
        var item = GarnishmentSegment.NewGarnishmentSegment("code-56", "label-94");
        item.Status = GarnishmentSegmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(GarnishmentSegmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        GarnishmentSegmentStartsInProposed();
        GarnishmentSegmentMovesToApproved();
        GarnishmentSegmentRejectsMovesFromWithdrawn();
    }
}
