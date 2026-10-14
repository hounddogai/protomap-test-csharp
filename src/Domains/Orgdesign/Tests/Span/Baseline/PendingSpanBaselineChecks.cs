namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PendingSpanBaselineChecks
{
    public static void PendingSpanBaselineStartsInProposed()
    {
        var item = PendingSpanBaseline.NewPendingSpanBaseline("code-361", "label-111");
        Check.Equal(item.Status, PendingSpanBaselineStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingSpanBaselineMovesToApproved()
    {
        var item = PendingSpanBaseline.NewPendingSpanBaseline("code-361", "label-111");
        try
        {
            item.MoveTo(PendingSpanBaselineStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingSpanBaselineStatus.Approved, "status");
    }

    public static void PendingSpanBaselineRejectsMovesFromWithdrawn()
    {
        var item = PendingSpanBaseline.NewPendingSpanBaseline("code-361", "label-111");
        item.Status = PendingSpanBaselineStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PendingSpanBaselineStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PendingSpanBaselineStartsInProposed();
        PendingSpanBaselineMovesToApproved();
        PendingSpanBaselineRejectsMovesFromWithdrawn();
    }
}
