namespace Acme.Workforce.Engagement.Tests.ActionPlan.Notice;

using Acme.Workforce.Engagement.ActionPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TieredActionPlanReservationChecks
{
    public static void TieredActionPlanReservationStartsInProposed()
    {
        var item = TieredActionPlanReservation.NewTieredActionPlanReservation("code-161", "name-536");
        Check.Equal(item.Status, TieredActionPlanReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredActionPlanReservationMovesToApproved()
    {
        var item = TieredActionPlanReservation.NewTieredActionPlanReservation("code-161", "name-536");
        try
        {
            item.MoveTo(TieredActionPlanReservationStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredActionPlanReservationStatus.Approved, "status");
    }

    public static void TieredActionPlanReservationRejectsMovesFromWithdrawn()
    {
        var item = TieredActionPlanReservation.NewTieredActionPlanReservation("code-161", "name-536");
        item.Status = TieredActionPlanReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TieredActionPlanReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TieredActionPlanReservationStartsInProposed();
        TieredActionPlanReservationMovesToApproved();
        TieredActionPlanReservationRejectsMovesFromWithdrawn();
    }
}
