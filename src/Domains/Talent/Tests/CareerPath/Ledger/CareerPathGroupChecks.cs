namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathGroupChecks
{
    public static void CareerPathGroupStartsInIdle()
    {
        var item = CareerPathGroup.NewCareerPathGroup("code-234", "label-921");
        Check.Equal(item.Status, CareerPathGroupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathGroupMovesToWarming()
    {
        var item = CareerPathGroup.NewCareerPathGroup("code-234", "label-921");
        try
        {
            item.MoveTo(CareerPathGroupStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathGroupStatus.Warming, "status");
    }

    public static void CareerPathGroupRejectsMovesFromStopped()
    {
        var item = CareerPathGroup.NewCareerPathGroup("code-234", "label-921");
        item.Status = CareerPathGroupStatus.Stopped;
        Check.Fails(() => item.MoveTo(CareerPathGroupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CareerPathGroupStartsInIdle();
        CareerPathGroupMovesToWarming();
        CareerPathGroupRejectsMovesFromStopped();
    }
}
