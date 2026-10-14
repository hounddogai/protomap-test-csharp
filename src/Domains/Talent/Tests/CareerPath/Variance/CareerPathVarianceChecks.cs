namespace Acme.Workforce.Talent.Tests.CareerPath.Variance;

using Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class CareerPathVarianceChecks
{
    public static void CareerPathVarianceStartsInOpen()
    {
        var item = CareerPathVariance.NewCareerPathVariance("code-416", "title-157");
        Check.Equal(item.Status, CareerPathVarianceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathVarianceMovesToResolved()
    {
        var item = CareerPathVariance.NewCareerPathVariance("code-416", "title-157");
        try
        {
            item.MoveTo(CareerPathVarianceStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathVarianceStatus.Resolved, "status");
    }

    public static void CareerPathVarianceRejectsMovesFromArchived()
    {
        var item = CareerPathVariance.NewCareerPathVariance("code-416", "title-157");
        item.Status = CareerPathVarianceStatus.Archived;
        Check.Fails(() => item.MoveTo(CareerPathVarianceStatus.Open), "move");
    }

    public static void RunAll()
    {
        CareerPathVarianceStartsInOpen();
        CareerPathVarianceMovesToResolved();
        CareerPathVarianceRejectsMovesFromArchived();
    }
}
