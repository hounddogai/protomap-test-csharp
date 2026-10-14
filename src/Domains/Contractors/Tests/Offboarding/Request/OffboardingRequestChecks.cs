namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class OffboardingRequestChecks
{
    public static void OffboardingRequestStartsInPlanned()
    {
        var item = OffboardingRequest.NewOffboardingRequest("code-544", "title-462");
        Check.Equal(item.Status, OffboardingRequestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingRequestMovesToCancelled()
    {
        var item = OffboardingRequest.NewOffboardingRequest("code-544", "title-462");
        try
        {
            item.MoveTo(OffboardingRequestStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingRequestStatus.Cancelled, "status");
    }

    public static void OffboardingRequestRejectsMovesFromCancelled()
    {
        var item = OffboardingRequest.NewOffboardingRequest("code-544", "title-462");
        item.Status = OffboardingRequestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(OffboardingRequestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        OffboardingRequestStartsInPlanned();
        OffboardingRequestMovesToCancelled();
        OffboardingRequestRejectsMovesFromCancelled();
    }
}
