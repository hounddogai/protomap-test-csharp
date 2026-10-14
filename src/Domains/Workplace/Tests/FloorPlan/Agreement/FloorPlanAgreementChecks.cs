namespace Acme.Workforce.Workplace.Tests.FloorPlan.Agreement;

using Acme.Workforce.Workplace.FloorPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class FloorPlanAgreementChecks
{
    public static void FloorPlanAgreementStartsInPlanned()
    {
        var item = FloorPlanAgreement.NewFloorPlanAgreement("code-131", "name-708");
        Check.Equal(item.Status, FloorPlanAgreementStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanAgreementMovesToScheduled()
    {
        var item = FloorPlanAgreement.NewFloorPlanAgreement("code-131", "name-708");
        try
        {
            item.MoveTo(FloorPlanAgreementStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanAgreementStatus.Scheduled, "status");
    }

    public static void FloorPlanAgreementRejectsMovesFromCancelled()
    {
        var item = FloorPlanAgreement.NewFloorPlanAgreement("code-131", "name-708");
        item.Status = FloorPlanAgreementStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FloorPlanAgreementStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FloorPlanAgreementStartsInPlanned();
        FloorPlanAgreementMovesToScheduled();
        FloorPlanAgreementRejectsMovesFromCancelled();
    }
}
