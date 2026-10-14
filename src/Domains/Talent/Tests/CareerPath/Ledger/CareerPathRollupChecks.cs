namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathRollupChecks
{
    public static void CareerPathRollupStartsInPlanned()
    {
        var item = CareerPathRollup.NewCareerPathRollup("code-350", "label-459");
        Check.Equal(item.Status, CareerPathRollupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathRollupMovesToInProgress()
    {
        var item = CareerPathRollup.NewCareerPathRollup("code-350", "label-459");
        try
        {
            item.MoveTo(CareerPathRollupStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathRollupStatus.InProgress, "status");
    }

    public static void CareerPathRollupRejectsMovesFromCancelled()
    {
        var item = CareerPathRollup.NewCareerPathRollup("code-350", "label-459");
        item.Status = CareerPathRollupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CareerPathRollupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CareerPathRollupStartsInPlanned();
        CareerPathRollupMovesToInProgress();
        CareerPathRollupRejectsMovesFromCancelled();
    }
}
