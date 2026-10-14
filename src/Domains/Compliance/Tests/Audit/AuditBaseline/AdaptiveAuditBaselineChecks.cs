namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class AdaptiveAuditBaselineChecks
{
    public static void AdaptiveAuditBaselineStartsInPlanned()
    {
        var item = AdaptiveAuditBaseline.NewAdaptiveAuditBaseline("code-639", "name-411");
        Check.Equal(item.Status, AdaptiveAuditBaselineStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveAuditBaselineMovesToInProgress()
    {
        var item = AdaptiveAuditBaseline.NewAdaptiveAuditBaseline("code-639", "name-411");
        try
        {
            item.MoveTo(AdaptiveAuditBaselineStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveAuditBaselineStatus.InProgress, "status");
    }

    public static void AdaptiveAuditBaselineRejectsMovesFromCancelled()
    {
        var item = AdaptiveAuditBaseline.NewAdaptiveAuditBaseline("code-639", "name-411");
        item.Status = AdaptiveAuditBaselineStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptiveAuditBaselineStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptiveAuditBaselineStartsInPlanned();
        AdaptiveAuditBaselineMovesToInProgress();
        AdaptiveAuditBaselineRejectsMovesFromCancelled();
    }
}
