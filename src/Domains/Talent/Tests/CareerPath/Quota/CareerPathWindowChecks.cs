namespace Acme.Workforce.Talent.Tests.CareerPath.Quota;

using Acme.Workforce.Talent.CareerPath.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class CareerPathWindowChecks
{
    public static void CareerPathWindowStartsInDraft()
    {
        var item = CareerPathWindow.NewCareerPathWindow("code-195", "title-626");
        Check.Equal(item.Status, CareerPathWindowStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathWindowMovesToPending()
    {
        var item = CareerPathWindow.NewCareerPathWindow("code-195", "title-626");
        try
        {
            item.MoveTo(CareerPathWindowStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathWindowStatus.Pending, "status");
    }

    public static void CareerPathWindowRejectsMovesFromClosed()
    {
        var item = CareerPathWindow.NewCareerPathWindow("code-195", "title-626");
        item.Status = CareerPathWindowStatus.Closed;
        Check.Fails(() => item.MoveTo(CareerPathWindowStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CareerPathWindowStartsInDraft();
        CareerPathWindowMovesToPending();
        CareerPathWindowRejectsMovesFromClosed();
    }
}
