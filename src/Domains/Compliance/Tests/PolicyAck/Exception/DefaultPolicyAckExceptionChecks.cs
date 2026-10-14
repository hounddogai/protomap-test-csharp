namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DefaultPolicyAckExceptionChecks
{
    public static void DefaultPolicyAckExceptionStartsInRequested()
    {
        var item = DefaultPolicyAckException.NewDefaultPolicyAckException("code-778", "name-943");
        Check.Equal(item.Status, DefaultPolicyAckExceptionStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPolicyAckExceptionMovesToRevoked()
    {
        var item = DefaultPolicyAckException.NewDefaultPolicyAckException("code-778", "name-943");
        try
        {
            item.MoveTo(DefaultPolicyAckExceptionStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPolicyAckExceptionStatus.Revoked, "status");
    }

    public static void DefaultPolicyAckExceptionRejectsMovesFromExpired()
    {
        var item = DefaultPolicyAckException.NewDefaultPolicyAckException("code-778", "name-943");
        item.Status = DefaultPolicyAckExceptionStatus.Expired;
        Check.Fails(() => item.MoveTo(DefaultPolicyAckExceptionStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DefaultPolicyAckExceptionStartsInRequested();
        DefaultPolicyAckExceptionMovesToRevoked();
        DefaultPolicyAckExceptionRejectsMovesFromExpired();
    }
}
