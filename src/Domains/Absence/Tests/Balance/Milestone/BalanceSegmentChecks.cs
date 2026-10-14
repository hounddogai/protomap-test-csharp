namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceSegmentChecks
{
    public static void BalanceSegmentStartsInDraft()
    {
        var item = BalanceSegment.NewBalanceSegment("code-9", "title-346");
        Check.Equal(item.Status, BalanceSegmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceSegmentMovesToActive()
    {
        var item = BalanceSegment.NewBalanceSegment("code-9", "title-346");
        try
        {
            item.MoveTo(BalanceSegmentStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceSegmentStatus.Active, "status");
    }

    public static void BalanceSegmentRejectsMovesFromClosed()
    {
        var item = BalanceSegment.NewBalanceSegment("code-9", "title-346");
        item.Status = BalanceSegmentStatus.Closed;
        Check.Fails(() => item.MoveTo(BalanceSegmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BalanceSegmentStartsInDraft();
        BalanceSegmentMovesToActive();
        BalanceSegmentRejectsMovesFromClosed();
    }
}
