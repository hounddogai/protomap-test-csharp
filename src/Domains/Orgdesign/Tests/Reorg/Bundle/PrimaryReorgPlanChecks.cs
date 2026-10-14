namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PrimaryReorgPlanChecks
{
    public static void PrimaryReorgPlanStartsInRequested()
    {
        var item = PrimaryReorgPlan.NewPrimaryReorgPlan("code-228", "label-369");
        Check.Equal(item.Status, PrimaryReorgPlanStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryReorgPlanMovesToGranted()
    {
        var item = PrimaryReorgPlan.NewPrimaryReorgPlan("code-228", "label-369");
        try
        {
            item.MoveTo(PrimaryReorgPlanStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryReorgPlanStatus.Granted, "status");
    }

    public static void PrimaryReorgPlanRejectsMovesFromExpired()
    {
        var item = PrimaryReorgPlan.NewPrimaryReorgPlan("code-228", "label-369");
        item.Status = PrimaryReorgPlanStatus.Expired;
        Check.Fails(() => item.MoveTo(PrimaryReorgPlanStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PrimaryReorgPlanStartsInRequested();
        PrimaryReorgPlanMovesToGranted();
        PrimaryReorgPlanRejectsMovesFromExpired();
    }
}
