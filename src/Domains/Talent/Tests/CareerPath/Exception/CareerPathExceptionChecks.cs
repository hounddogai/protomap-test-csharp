namespace Acme.Workforce.Talent.Tests.CareerPath.Exception;

using Acme.Workforce.Talent.CareerPath.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class CareerPathExceptionChecks
{
    public static void CareerPathExceptionStartsInProposed()
    {
        var item = CareerPathException.NewCareerPathException("code-782", "title-483");
        Check.Equal(item.Status, CareerPathExceptionStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathExceptionMovesToRejected()
    {
        var item = CareerPathException.NewCareerPathException("code-782", "title-483");
        try
        {
            item.MoveTo(CareerPathExceptionStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathExceptionStatus.Rejected, "status");
    }

    public static void CareerPathExceptionRejectsMovesFromWithdrawn()
    {
        var item = CareerPathException.NewCareerPathException("code-782", "title-483");
        item.Status = CareerPathExceptionStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CareerPathExceptionStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CareerPathExceptionStartsInProposed();
        CareerPathExceptionMovesToRejected();
        CareerPathExceptionRejectsMovesFromWithdrawn();
    }
}
