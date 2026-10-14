namespace Acme.Workforce.Talent.Tests.Mobility.RecordItem;

using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DefaultMobilityRecordChecks
{
    public static void DefaultMobilityRecordStartsInProposed()
    {
        var item = DefaultMobilityRecord.NewDefaultMobilityRecord("code-121", "label-14");
        Check.Equal(item.Status, DefaultMobilityRecordStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultMobilityRecordMovesToApproved()
    {
        var item = DefaultMobilityRecord.NewDefaultMobilityRecord("code-121", "label-14");
        try
        {
            item.MoveTo(DefaultMobilityRecordStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultMobilityRecordStatus.Approved, "status");
    }

    public static void DefaultMobilityRecordRejectsMovesFromWithdrawn()
    {
        var item = DefaultMobilityRecord.NewDefaultMobilityRecord("code-121", "label-14");
        item.Status = DefaultMobilityRecordStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultMobilityRecordStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultMobilityRecordStartsInProposed();
        DefaultMobilityRecordMovesToApproved();
        DefaultMobilityRecordRejectsMovesFromWithdrawn();
    }
}
