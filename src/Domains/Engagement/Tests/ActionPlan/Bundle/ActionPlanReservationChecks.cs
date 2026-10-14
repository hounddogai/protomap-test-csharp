namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ActionPlanReservationChecks
{
    public static void ActionPlanReservationStartsInPlanned()
    {
        var item = ActionPlanReservation.NewActionPlanReservation("code-13", "title-987");
        Check.Equal(item.Status, ActionPlanReservationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanReservationMovesToScheduled()
    {
        var item = ActionPlanReservation.NewActionPlanReservation("code-13", "title-987");
        try
        {
            item.MoveTo(ActionPlanReservationStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanReservationStatus.Scheduled, "status");
    }

    public static void ActionPlanReservationRejectsMovesFromCancelled()
    {
        var item = ActionPlanReservation.NewActionPlanReservation("code-13", "title-987");
        item.Status = ActionPlanReservationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ActionPlanReservationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ActionPlanReservationStartsInPlanned();
        ActionPlanReservationMovesToScheduled();
        ActionPlanReservationRejectsMovesFromCancelled();
    }
}
