namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PolicyAckMetricChecks
{
    public static void PolicyAckMetricStartsInNew()
    {
        var item = PolicyAckMetric.NewPolicyAckMetric("code-432", "label-840");
        Check.Equal(item.Status, PolicyAckMetricStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckMetricMovesToRunning()
    {
        var item = PolicyAckMetric.NewPolicyAckMetric("code-432", "label-840");
        try
        {
            item.MoveTo(PolicyAckMetricStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckMetricStatus.Running, "status");
    }

    public static void PolicyAckMetricRejectsMovesFromFailed()
    {
        var item = PolicyAckMetric.NewPolicyAckMetric("code-432", "label-840");
        item.Status = PolicyAckMetricStatus.Failed;
        Check.Fails(() => item.MoveTo(PolicyAckMetricStatus.New), "move");
    }

    public static void RunAll()
    {
        PolicyAckMetricStartsInNew();
        PolicyAckMetricMovesToRunning();
        PolicyAckMetricRejectsMovesFromFailed();
    }
}
