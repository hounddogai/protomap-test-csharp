namespace Acme.Workforce.Contractors.Tests.Offboarding.Bundle;

using Acme.Workforce.Contractors.Offboarding.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class OffboardingAssignmentChecks
{
    public static void OffboardingAssignmentStartsInNew()
    {
        var item = OffboardingAssignment.NewOffboardingAssignment("code-897", "label-188");
        Check.Equal(item.Status, OffboardingAssignmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingAssignmentMovesToQueued()
    {
        var item = OffboardingAssignment.NewOffboardingAssignment("code-897", "label-188");
        try
        {
            item.MoveTo(OffboardingAssignmentStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingAssignmentStatus.Queued, "status");
    }

    public static void OffboardingAssignmentRejectsMovesFromFailed()
    {
        var item = OffboardingAssignment.NewOffboardingAssignment("code-897", "label-188");
        item.Status = OffboardingAssignmentStatus.Failed;
        Check.Fails(() => item.MoveTo(OffboardingAssignmentStatus.New), "move");
    }

    public static void RunAll()
    {
        OffboardingAssignmentStartsInNew();
        OffboardingAssignmentMovesToQueued();
        OffboardingAssignmentRejectsMovesFromFailed();
    }
}
