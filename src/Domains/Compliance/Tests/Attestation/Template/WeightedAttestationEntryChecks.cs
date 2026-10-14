namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WeightedAttestationEntryChecks
{
    public static void WeightedAttestationEntryStartsInIdle()
    {
        var item = WeightedAttestationEntry.NewWeightedAttestationEntry("code-105", "label-3");
        Check.Equal(item.Status, WeightedAttestationEntryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAttestationEntryMovesToReady()
    {
        var item = WeightedAttestationEntry.NewWeightedAttestationEntry("code-105", "label-3");
        try
        {
            item.MoveTo(WeightedAttestationEntryStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAttestationEntryStatus.Ready, "status");
    }

    public static void WeightedAttestationEntryRejectsMovesFromStopped()
    {
        var item = WeightedAttestationEntry.NewWeightedAttestationEntry("code-105", "label-3");
        item.Status = WeightedAttestationEntryStatus.Stopped;
        Check.Fails(() => item.MoveTo(WeightedAttestationEntryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WeightedAttestationEntryStartsInIdle();
        WeightedAttestationEntryMovesToReady();
        WeightedAttestationEntryRejectsMovesFromStopped();
    }
}
