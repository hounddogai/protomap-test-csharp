namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class CostModelAgreementChecks
{
    public static void CostModelAgreementStartsInPlanned()
    {
        var item = CostModelAgreement.NewCostModelAgreement("code-344", "name-143");
        Check.Equal(item.Status, CostModelAgreementStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelAgreementMovesToInProgress()
    {
        var item = CostModelAgreement.NewCostModelAgreement("code-344", "name-143");
        try
        {
            item.MoveTo(CostModelAgreementStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelAgreementStatus.InProgress, "status");
    }

    public static void CostModelAgreementRejectsMovesFromCancelled()
    {
        var item = CostModelAgreement.NewCostModelAgreement("code-344", "name-143");
        item.Status = CostModelAgreementStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CostModelAgreementStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CostModelAgreementStartsInPlanned();
        CostModelAgreementMovesToInProgress();
        CostModelAgreementRejectsMovesFromCancelled();
    }
}
