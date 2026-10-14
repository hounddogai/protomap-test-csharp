namespace Acme.Workforce.Workplace.Tests.FloorPlan.Metric;

using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedFloorPlanAgreementChecks
{
    public static void CappedFloorPlanAgreementStartsInProposed()
    {
        var item = CappedFloorPlanAgreement.NewCappedFloorPlanAgreement("code-690", "name-895");
        Check.Equal(item.Status, CappedFloorPlanAgreementStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedFloorPlanAgreementMovesToWithdrawn()
    {
        var item = CappedFloorPlanAgreement.NewCappedFloorPlanAgreement("code-690", "name-895");
        try
        {
            item.MoveTo(CappedFloorPlanAgreementStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedFloorPlanAgreementStatus.Withdrawn, "status");
    }

    public static void CappedFloorPlanAgreementRejectsMovesFromWithdrawn()
    {
        var item = CappedFloorPlanAgreement.NewCappedFloorPlanAgreement("code-690", "name-895");
        item.Status = CappedFloorPlanAgreementStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CappedFloorPlanAgreementStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CappedFloorPlanAgreementStartsInProposed();
        CappedFloorPlanAgreementMovesToWithdrawn();
        CappedFloorPlanAgreementRejectsMovesFromWithdrawn();
    }
}
