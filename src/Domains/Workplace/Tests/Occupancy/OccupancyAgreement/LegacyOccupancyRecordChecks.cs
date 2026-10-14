namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAgreement;

using Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

/// <summary>Checks of occupancy agreement code, run by the project's runner.</summary>
public static class LegacyOccupancyRecordChecks
{
    public static void LegacyOccupancyRecordStartsInCreated()
    {
        var item = LegacyOccupancyRecord.NewLegacyOccupancyRecord("code-751", "name-802");
        Check.Equal(item.Status, LegacyOccupancyRecordStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyOccupancyRecordMovesToRetired()
    {
        var item = LegacyOccupancyRecord.NewLegacyOccupancyRecord("code-751", "name-802");
        try
        {
            item.MoveTo(LegacyOccupancyRecordStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyOccupancyRecordStatus.Retired, "status");
    }

    public static void LegacyOccupancyRecordRejectsMovesFromRetired()
    {
        var item = LegacyOccupancyRecord.NewLegacyOccupancyRecord("code-751", "name-802");
        item.Status = LegacyOccupancyRecordStatus.Retired;
        Check.Fails(() => item.MoveTo(LegacyOccupancyRecordStatus.Created), "move");
    }

    public static void RunAll()
    {
        LegacyOccupancyRecordStartsInCreated();
        LegacyOccupancyRecordMovesToRetired();
        LegacyOccupancyRecordRejectsMovesFromRetired();
    }
}
