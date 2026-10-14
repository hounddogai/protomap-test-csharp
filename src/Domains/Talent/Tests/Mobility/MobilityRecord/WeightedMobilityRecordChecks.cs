namespace Acme.Workforce.Talent.Tests.Mobility.MobilityRecord;

using Acme.Workforce.Talent.Mobility.MobilityRecord;

/// <summary>Checks of mobility record code, run by the project's runner.</summary>
public static class WeightedMobilityRecordChecks
{
    public static void WeightedMobilityRecordStartsInRequested()
    {
        var item = WeightedMobilityRecord.NewWeightedMobilityRecord("code-479", "name-285");
        Check.Equal(item.Status, WeightedMobilityRecordStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedMobilityRecordMovesToGranted()
    {
        var item = WeightedMobilityRecord.NewWeightedMobilityRecord("code-479", "name-285");
        try
        {
            item.MoveTo(WeightedMobilityRecordStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedMobilityRecordStatus.Granted, "status");
    }

    public static void WeightedMobilityRecordRejectsMovesFromExpired()
    {
        var item = WeightedMobilityRecord.NewWeightedMobilityRecord("code-479", "name-285");
        item.Status = WeightedMobilityRecordStatus.Expired;
        Check.Fails(() => item.MoveTo(WeightedMobilityRecordStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WeightedMobilityRecordStartsInRequested();
        WeightedMobilityRecordMovesToGranted();
        WeightedMobilityRecordRejectsMovesFromExpired();
    }
}
