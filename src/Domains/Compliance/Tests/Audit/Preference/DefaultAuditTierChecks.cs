namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultAuditTierChecks
{
    public static void DefaultAuditTierStartsInDraft()
    {
        var item = DefaultAuditTier.NewDefaultAuditTier("code-926", "label-709");
        Check.Equal(item.Status, DefaultAuditTierStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAuditTierMovesToActive()
    {
        var item = DefaultAuditTier.NewDefaultAuditTier("code-926", "label-709");
        try
        {
            item.MoveTo(DefaultAuditTierStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAuditTierStatus.Active, "status");
    }

    public static void DefaultAuditTierRejectsMovesFromClosed()
    {
        var item = DefaultAuditTier.NewDefaultAuditTier("code-926", "label-709");
        item.Status = DefaultAuditTierStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultAuditTierStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultAuditTierStartsInDraft();
        DefaultAuditTierMovesToActive();
        DefaultAuditTierRejectsMovesFromClosed();
    }
}
