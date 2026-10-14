namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BonusPoolRecordChecks
{
    public static void BonusPoolRecordStartsInOpen()
    {
        var item = BonusPoolRecord.NewBonusPoolRecord("code-279", "label-173");
        Check.Equal(item.Status, BonusPoolRecordStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolRecordMovesToInReview()
    {
        var item = BonusPoolRecord.NewBonusPoolRecord("code-279", "label-173");
        try
        {
            item.MoveTo(BonusPoolRecordStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolRecordStatus.InReview, "status");
    }

    public static void BonusPoolRecordRejectsMovesFromArchived()
    {
        var item = BonusPoolRecord.NewBonusPoolRecord("code-279", "label-173");
        item.Status = BonusPoolRecordStatus.Archived;
        Check.Fails(() => item.MoveTo(BonusPoolRecordStatus.Open), "move");
    }

    public static void RunAll()
    {
        BonusPoolRecordStartsInOpen();
        BonusPoolRecordMovesToInReview();
        BonusPoolRecordRejectsMovesFromArchived();
    }
}
