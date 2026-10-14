namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FallbackExceptionAssignmentChecks
{
    public static void FallbackExceptionAssignmentStartsInCreated()
    {
        var item = FallbackExceptionAssignment.NewFallbackExceptionAssignment("code-45", "name-352");
        Check.Equal(item.Status, FallbackExceptionAssignmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackExceptionAssignmentMovesToValidated()
    {
        var item = FallbackExceptionAssignment.NewFallbackExceptionAssignment("code-45", "name-352");
        try
        {
            item.MoveTo(FallbackExceptionAssignmentStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackExceptionAssignmentStatus.Validated, "status");
    }

    public static void FallbackExceptionAssignmentRejectsMovesFromRetired()
    {
        var item = FallbackExceptionAssignment.NewFallbackExceptionAssignment("code-45", "name-352");
        item.Status = FallbackExceptionAssignmentStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackExceptionAssignmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackExceptionAssignmentStartsInCreated();
        FallbackExceptionAssignmentMovesToValidated();
        FallbackExceptionAssignmentRejectsMovesFromRetired();
    }
}
