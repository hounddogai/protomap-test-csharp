namespace Acme.Workforce.Talent.Tests.Mobility.MobilityAudit;

using Acme.Workforce.Talent.Mobility.MobilityAudit;

/// <summary>Checks of mobility audit code, run by the project's runner.</summary>
public static class LegacyMobilityOverrideChecks
{
    public static void LegacyMobilityOverrideStartsInDraft()
    {
        var item = LegacyMobilityOverride.NewLegacyMobilityOverride("code-767", "label-826");
        Check.Equal(item.Status, LegacyMobilityOverrideStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyMobilityOverrideMovesToPending()
    {
        var item = LegacyMobilityOverride.NewLegacyMobilityOverride("code-767", "label-826");
        try
        {
            item.MoveTo(LegacyMobilityOverrideStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyMobilityOverrideStatus.Pending, "status");
    }

    public static void LegacyMobilityOverrideRejectsMovesFromClosed()
    {
        var item = LegacyMobilityOverride.NewLegacyMobilityOverride("code-767", "label-826");
        item.Status = LegacyMobilityOverrideStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyMobilityOverrideStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyMobilityOverrideStartsInDraft();
        LegacyMobilityOverrideMovesToPending();
        LegacyMobilityOverrideRejectsMovesFromClosed();
    }
}
