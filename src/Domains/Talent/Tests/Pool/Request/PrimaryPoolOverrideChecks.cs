namespace Acme.Workforce.Talent.Tests.Pool.Request;

using Acme.Workforce.Talent.Pool.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PrimaryPoolOverrideChecks
{
    public static void PrimaryPoolOverrideStartsInRequested()
    {
        var item = PrimaryPoolOverride.NewPrimaryPoolOverride("code-329", "name-393");
        Check.Equal(item.Status, PrimaryPoolOverrideStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryPoolOverrideMovesToRevoked()
    {
        var item = PrimaryPoolOverride.NewPrimaryPoolOverride("code-329", "name-393");
        try
        {
            item.MoveTo(PrimaryPoolOverrideStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryPoolOverrideStatus.Revoked, "status");
    }

    public static void PrimaryPoolOverrideRejectsMovesFromExpired()
    {
        var item = PrimaryPoolOverride.NewPrimaryPoolOverride("code-329", "name-393");
        item.Status = PrimaryPoolOverrideStatus.Expired;
        Check.Fails(() => item.MoveTo(PrimaryPoolOverrideStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PrimaryPoolOverrideStartsInRequested();
        PrimaryPoolOverrideMovesToRevoked();
        PrimaryPoolOverrideRejectsMovesFromExpired();
    }
}
