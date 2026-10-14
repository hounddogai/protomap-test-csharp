namespace Acme.Workforce.Assets.Tests.Assignment.Agreement;

using Acme.Workforce.Assets.Assignment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssignmentPolicyChecks
{
    public static void AssignmentPolicyStartsInCreated()
    {
        var item = AssignmentPolicy.NewAssignmentPolicy("code-151", "title-788");
        Check.Equal(item.Status, AssignmentPolicyStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentPolicyMovesToValidated()
    {
        var item = AssignmentPolicy.NewAssignmentPolicy("code-151", "title-788");
        try
        {
            item.MoveTo(AssignmentPolicyStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentPolicyStatus.Validated, "status");
    }

    public static void AssignmentPolicyRejectsMovesFromRetired()
    {
        var item = AssignmentPolicy.NewAssignmentPolicy("code-151", "title-788");
        item.Status = AssignmentPolicyStatus.Retired;
        Check.Fails(() => item.MoveTo(AssignmentPolicyStatus.Created), "move");
    }

    public static void RunAll()
    {
        AssignmentPolicyStartsInCreated();
        AssignmentPolicyMovesToValidated();
        AssignmentPolicyRejectsMovesFromRetired();
    }
}
