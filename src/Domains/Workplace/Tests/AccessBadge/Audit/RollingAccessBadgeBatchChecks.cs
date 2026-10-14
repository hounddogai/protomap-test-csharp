namespace Acme.Workforce.Workplace.Tests.AccessBadge.Audit;

using Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RollingAccessBadgeBatchChecks
{
    public static void RollingAccessBadgeBatchStartsInOpen()
    {
        var item = RollingAccessBadgeBatch.NewRollingAccessBadgeBatch("code-701", "title-332");
        Check.Equal(item.Status, RollingAccessBadgeBatchStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingAccessBadgeBatchMovesToArchived()
    {
        var item = RollingAccessBadgeBatch.NewRollingAccessBadgeBatch("code-701", "title-332");
        try
        {
            item.MoveTo(RollingAccessBadgeBatchStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingAccessBadgeBatchStatus.Archived, "status");
    }

    public static void RollingAccessBadgeBatchRejectsMovesFromArchived()
    {
        var item = RollingAccessBadgeBatch.NewRollingAccessBadgeBatch("code-701", "title-332");
        item.Status = RollingAccessBadgeBatchStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingAccessBadgeBatchStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingAccessBadgeBatchStartsInOpen();
        RollingAccessBadgeBatchMovesToArchived();
        RollingAccessBadgeBatchRejectsMovesFromArchived();
    }
}
