namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PoolRecordChecks
{
    public static void PoolRecordStartsInCreated()
    {
        var item = PoolRecord.NewPoolRecord("code-705", "title-184");
        Check.Equal(item.Status, PoolRecordStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolRecordMovesToPublished()
    {
        var item = PoolRecord.NewPoolRecord("code-705", "title-184");
        try
        {
            item.MoveTo(PoolRecordStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolRecordStatus.Published, "status");
    }

    public static void PoolRecordRejectsMovesFromRetired()
    {
        var item = PoolRecord.NewPoolRecord("code-705", "title-184");
        item.Status = PoolRecordStatus.Retired;
        Check.Fails(() => item.MoveTo(PoolRecordStatus.Created), "move");
    }

    public static void RunAll()
    {
        PoolRecordStartsInCreated();
        PoolRecordMovesToPublished();
        PoolRecordRejectsMovesFromRetired();
    }
}
