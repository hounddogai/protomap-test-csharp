namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FallbackCostModelRequestChecks
{
    public static void FallbackCostModelRequestStartsInNew()
    {
        var item = FallbackCostModelRequest.NewFallbackCostModelRequest("code-145", "label-743");
        Check.Equal(item.Status, FallbackCostModelRequestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackCostModelRequestMovesToRunning()
    {
        var item = FallbackCostModelRequest.NewFallbackCostModelRequest("code-145", "label-743");
        try
        {
            item.MoveTo(FallbackCostModelRequestStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackCostModelRequestStatus.Running, "status");
    }

    public static void FallbackCostModelRequestRejectsMovesFromFailed()
    {
        var item = FallbackCostModelRequest.NewFallbackCostModelRequest("code-145", "label-743");
        item.Status = FallbackCostModelRequestStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackCostModelRequestStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackCostModelRequestStartsInNew();
        FallbackCostModelRequestMovesToRunning();
        FallbackCostModelRequestRejectsMovesFromFailed();
    }
}
