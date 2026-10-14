namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class FallbackRefreshPolicyChecks
{
    public static void FallbackRefreshPolicyStartsInNew()
    {
        var item = FallbackRefreshPolicy.NewFallbackRefreshPolicy("code-172", "label-690");
        Check.Equal(item.Status, FallbackRefreshPolicyStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackRefreshPolicyMovesToQueued()
    {
        var item = FallbackRefreshPolicy.NewFallbackRefreshPolicy("code-172", "label-690");
        try
        {
            item.MoveTo(FallbackRefreshPolicyStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackRefreshPolicyStatus.Queued, "status");
    }

    public static void FallbackRefreshPolicyRejectsMovesFromFailed()
    {
        var item = FallbackRefreshPolicy.NewFallbackRefreshPolicy("code-172", "label-690");
        item.Status = FallbackRefreshPolicyStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackRefreshPolicyStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackRefreshPolicyStartsInNew();
        FallbackRefreshPolicyMovesToQueued();
        FallbackRefreshPolicyRejectsMovesFromFailed();
    }
}
