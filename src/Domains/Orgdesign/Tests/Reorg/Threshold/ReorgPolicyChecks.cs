namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ReorgPolicyChecks
{
    public static void ReorgPolicyStartsInNew()
    {
        var item = ReorgPolicy.NewReorgPolicy("code-883", "label-212");
        Check.Equal(item.Status, ReorgPolicyStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgPolicyMovesToDone()
    {
        var item = ReorgPolicy.NewReorgPolicy("code-883", "label-212");
        try
        {
            item.MoveTo(ReorgPolicyStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgPolicyStatus.Done, "status");
    }

    public static void ReorgPolicyRejectsMovesFromFailed()
    {
        var item = ReorgPolicy.NewReorgPolicy("code-883", "label-212");
        item.Status = ReorgPolicyStatus.Failed;
        Check.Fails(() => item.MoveTo(ReorgPolicyStatus.New), "move");
    }

    public static void RunAll()
    {
        ReorgPolicyStartsInNew();
        ReorgPolicyMovesToDone();
        ReorgPolicyRejectsMovesFromFailed();
    }
}
