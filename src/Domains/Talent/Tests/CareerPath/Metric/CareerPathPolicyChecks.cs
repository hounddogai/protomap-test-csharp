namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CareerPathPolicyChecks
{
    public static void CareerPathPolicyStartsInProposed()
    {
        var item = CareerPathPolicy.NewCareerPathPolicy("code-309", "label-28");
        Check.Equal(item.Status, CareerPathPolicyStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathPolicyMovesToRejected()
    {
        var item = CareerPathPolicy.NewCareerPathPolicy("code-309", "label-28");
        try
        {
            item.MoveTo(CareerPathPolicyStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathPolicyStatus.Rejected, "status");
    }

    public static void CareerPathPolicyRejectsMovesFromWithdrawn()
    {
        var item = CareerPathPolicy.NewCareerPathPolicy("code-309", "label-28");
        item.Status = CareerPathPolicyStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CareerPathPolicyStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CareerPathPolicyStartsInProposed();
        CareerPathPolicyMovesToRejected();
        CareerPathPolicyRejectsMovesFromWithdrawn();
    }
}
