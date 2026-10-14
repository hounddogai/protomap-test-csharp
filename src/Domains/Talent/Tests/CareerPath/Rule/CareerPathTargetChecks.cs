namespace Acme.Workforce.Talent.Tests.CareerPath.Rule;

using Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class CareerPathTargetChecks
{
    public static void CareerPathTargetStartsInNew()
    {
        var item = CareerPathTarget.NewCareerPathTarget("code-129", "title-962");
        Check.Equal(item.Status, CareerPathTargetStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathTargetMovesToQueued()
    {
        var item = CareerPathTarget.NewCareerPathTarget("code-129", "title-962");
        try
        {
            item.MoveTo(CareerPathTargetStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathTargetStatus.Queued, "status");
    }

    public static void CareerPathTargetRejectsMovesFromFailed()
    {
        var item = CareerPathTarget.NewCareerPathTarget("code-129", "title-962");
        item.Status = CareerPathTargetStatus.Failed;
        Check.Fails(() => item.MoveTo(CareerPathTargetStatus.New), "move");
    }

    public static void RunAll()
    {
        CareerPathTargetStartsInNew();
        CareerPathTargetMovesToQueued();
        CareerPathTargetRejectsMovesFromFailed();
    }
}
