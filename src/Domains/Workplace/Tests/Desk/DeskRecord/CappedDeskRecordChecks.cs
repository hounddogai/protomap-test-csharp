namespace Acme.Workforce.Workplace.Tests.Desk.DeskRecord;

using Acme.Workforce.Workplace.Desk.DeskRecord;

/// <summary>Checks of desk record code, run by the project's runner.</summary>
public static class CappedDeskRecordChecks
{
    public static void CappedDeskRecordStartsInCreated()
    {
        var item = CappedDeskRecord.NewCappedDeskRecord("code-331", "name-917");
        Check.Equal(item.Status, CappedDeskRecordStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedDeskRecordMovesToValidated()
    {
        var item = CappedDeskRecord.NewCappedDeskRecord("code-331", "name-917");
        try
        {
            item.MoveTo(CappedDeskRecordStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedDeskRecordStatus.Validated, "status");
    }

    public static void CappedDeskRecordRejectsMovesFromRetired()
    {
        var item = CappedDeskRecord.NewCappedDeskRecord("code-331", "name-917");
        item.Status = CappedDeskRecordStatus.Retired;
        Check.Fails(() => item.MoveTo(CappedDeskRecordStatus.Created), "move");
    }

    public static void RunAll()
    {
        CappedDeskRecordStartsInCreated();
        CappedDeskRecordMovesToValidated();
        CappedDeskRecordRejectsMovesFromRetired();
    }
}
