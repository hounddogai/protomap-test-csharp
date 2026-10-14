namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class CostModelRecordChecks
{
    public static void CostModelRecordStartsInProposed()
    {
        var item = CostModelRecord.NewCostModelRecord("code-549", "title-753");
        Check.Equal(item.Status, CostModelRecordStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelRecordMovesToRejected()
    {
        var item = CostModelRecord.NewCostModelRecord("code-549", "title-753");
        try
        {
            item.MoveTo(CostModelRecordStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelRecordStatus.Rejected, "status");
    }

    public static void CostModelRecordRejectsMovesFromWithdrawn()
    {
        var item = CostModelRecord.NewCostModelRecord("code-549", "title-753");
        item.Status = CostModelRecordStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CostModelRecordStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CostModelRecordStartsInProposed();
        CostModelRecordMovesToRejected();
        CostModelRecordRejectsMovesFromWithdrawn();
    }
}
