namespace Acme.Workforce.Assets.Tests.Assignment.Request;

using Acme.Workforce.Assets.Assignment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AssignmentRequestChecks
{
    public static void AssignmentRequestStartsInRequested()
    {
        var item = AssignmentRequest.NewAssignmentRequest("code-462", "label-688");
        Check.Equal(item.Status, AssignmentRequestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentRequestMovesToRevoked()
    {
        var item = AssignmentRequest.NewAssignmentRequest("code-462", "label-688");
        try
        {
            item.MoveTo(AssignmentRequestStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentRequestStatus.Revoked, "status");
    }

    public static void AssignmentRequestRejectsMovesFromExpired()
    {
        var item = AssignmentRequest.NewAssignmentRequest("code-462", "label-688");
        item.Status = AssignmentRequestStatus.Expired;
        Check.Fails(() => item.MoveTo(AssignmentRequestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AssignmentRequestStartsInRequested();
        AssignmentRequestMovesToRevoked();
        AssignmentRequestRejectsMovesFromExpired();
    }
}
