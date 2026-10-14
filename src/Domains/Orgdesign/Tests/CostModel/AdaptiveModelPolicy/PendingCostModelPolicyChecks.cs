namespace Acme.Workforce.Orgdesign.Tests.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

/// <summary>Checks of adaptive model policy code, run by the project's runner.</summary>
public static class PendingCostModelPolicyChecks
{
    public static void PendingCostModelPolicyStartsInRequested()
    {
        var item = PendingCostModelPolicy.NewPendingCostModelPolicy("code-411", "label-378");
        Check.Equal(item.Status, PendingCostModelPolicyStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingCostModelPolicyMovesToGranted()
    {
        var item = PendingCostModelPolicy.NewPendingCostModelPolicy("code-411", "label-378");
        try
        {
            item.MoveTo(PendingCostModelPolicyStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingCostModelPolicyStatus.Granted, "status");
    }

    public static void PendingCostModelPolicyRejectsMovesFromExpired()
    {
        var item = PendingCostModelPolicy.NewPendingCostModelPolicy("code-411", "label-378");
        item.Status = PendingCostModelPolicyStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingCostModelPolicyStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingCostModelPolicyStartsInRequested();
        PendingCostModelPolicyMovesToGranted();
        PendingCostModelPolicyRejectsMovesFromExpired();
    }
}
