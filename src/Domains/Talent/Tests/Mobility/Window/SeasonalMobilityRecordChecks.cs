namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SeasonalMobilityRecordChecks
{
    public static void SeasonalMobilityRecordStartsInPlanned()
    {
        var item = SeasonalMobilityRecord.NewSeasonalMobilityRecord("code-529", "label-692");
        Check.Equal(item.Status, SeasonalMobilityRecordStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalMobilityRecordMovesToCompleted()
    {
        var item = SeasonalMobilityRecord.NewSeasonalMobilityRecord("code-529", "label-692");
        try
        {
            item.MoveTo(SeasonalMobilityRecordStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalMobilityRecordStatus.Completed, "status");
    }

    public static void SeasonalMobilityRecordRejectsMovesFromCancelled()
    {
        var item = SeasonalMobilityRecord.NewSeasonalMobilityRecord("code-529", "label-692");
        item.Status = SeasonalMobilityRecordStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalMobilityRecordStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalMobilityRecordStartsInPlanned();
        SeasonalMobilityRecordMovesToCompleted();
        SeasonalMobilityRecordRejectsMovesFromCancelled();
    }
}
