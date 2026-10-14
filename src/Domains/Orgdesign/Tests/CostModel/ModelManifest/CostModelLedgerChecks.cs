namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class CostModelLedgerChecks
{
    public static void CostModelLedgerStartsInProposed()
    {
        var item = CostModelLedger.NewCostModelLedger("code-841", "label-66");
        Check.Equal(item.Status, CostModelLedgerStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelLedgerMovesToApproved()
    {
        var item = CostModelLedger.NewCostModelLedger("code-841", "label-66");
        try
        {
            item.MoveTo(CostModelLedgerStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelLedgerStatus.Approved, "status");
    }

    public static void CostModelLedgerRejectsMovesFromWithdrawn()
    {
        var item = CostModelLedger.NewCostModelLedger("code-841", "label-66");
        item.Status = CostModelLedgerStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CostModelLedgerStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CostModelLedgerStartsInProposed();
        CostModelLedgerMovesToApproved();
        CostModelLedgerRejectsMovesFromWithdrawn();
    }
}
