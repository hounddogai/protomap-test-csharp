namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TieredPolicyAckVarianceChecks
{
    public static void TieredPolicyAckVarianceStartsInIdle()
    {
        var item = TieredPolicyAckVariance.NewTieredPolicyAckVariance("code-911", "label-24");
        Check.Equal(item.Status, TieredPolicyAckVarianceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPolicyAckVarianceMovesToWarming()
    {
        var item = TieredPolicyAckVariance.NewTieredPolicyAckVariance("code-911", "label-24");
        try
        {
            item.MoveTo(TieredPolicyAckVarianceStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPolicyAckVarianceStatus.Warming, "status");
    }

    public static void TieredPolicyAckVarianceRejectsMovesFromStopped()
    {
        var item = TieredPolicyAckVariance.NewTieredPolicyAckVariance("code-911", "label-24");
        item.Status = TieredPolicyAckVarianceStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredPolicyAckVarianceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredPolicyAckVarianceStartsInIdle();
        TieredPolicyAckVarianceMovesToWarming();
        TieredPolicyAckVarianceRejectsMovesFromStopped();
    }
}
