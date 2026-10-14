namespace Acme.Workforce.Compliance.Tests.PolicyAck.Reservation;

using Acme.Workforce.Compliance.PolicyAck.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TieredPolicyAckOverrideChecks
{
    public static void TieredPolicyAckOverrideStartsInCreated()
    {
        var item = TieredPolicyAckOverride.NewTieredPolicyAckOverride("code-69", "label-144");
        Check.Equal(item.Status, TieredPolicyAckOverrideStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPolicyAckOverrideMovesToPublished()
    {
        var item = TieredPolicyAckOverride.NewTieredPolicyAckOverride("code-69", "label-144");
        try
        {
            item.MoveTo(TieredPolicyAckOverrideStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPolicyAckOverrideStatus.Published, "status");
    }

    public static void TieredPolicyAckOverrideRejectsMovesFromRetired()
    {
        var item = TieredPolicyAckOverride.NewTieredPolicyAckOverride("code-69", "label-144");
        item.Status = TieredPolicyAckOverrideStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredPolicyAckOverrideStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredPolicyAckOverrideStartsInCreated();
        TieredPolicyAckOverrideMovesToPublished();
        TieredPolicyAckOverrideRejectsMovesFromRetired();
    }
}
