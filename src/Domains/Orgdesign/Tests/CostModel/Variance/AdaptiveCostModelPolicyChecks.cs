namespace Acme.Workforce.Orgdesign.Tests.CostModel.Variance;

using Acme.Workforce.Orgdesign.CostModel.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveCostModelPolicyChecks
{
    public static void AdaptiveCostModelPolicyStartsInIdle()
    {
        var item = AdaptiveCostModelPolicy.NewAdaptiveCostModelPolicy("code-769", "label-350");
        Check.Equal(item.Status, AdaptiveCostModelPolicyStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveCostModelPolicyMovesToWarming()
    {
        var item = AdaptiveCostModelPolicy.NewAdaptiveCostModelPolicy("code-769", "label-350");
        try
        {
            item.MoveTo(AdaptiveCostModelPolicyStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveCostModelPolicyStatus.Warming, "status");
    }

    public static void AdaptiveCostModelPolicyRejectsMovesFromStopped()
    {
        var item = AdaptiveCostModelPolicy.NewAdaptiveCostModelPolicy("code-769", "label-350");
        item.Status = AdaptiveCostModelPolicyStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveCostModelPolicyStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveCostModelPolicyStartsInIdle();
        AdaptiveCostModelPolicyMovesToWarming();
        AdaptiveCostModelPolicyRejectsMovesFromStopped();
    }
}
