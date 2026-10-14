namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RegionalReorgPolicyChecks
{
    public static void RegionalReorgPolicyStartsInNew()
    {
        var item = RegionalReorgPolicy.NewRegionalReorgPolicy("code-478", "name-516");
        Check.Equal(item.Status, RegionalReorgPolicyStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalReorgPolicyMovesToQueued()
    {
        var item = RegionalReorgPolicy.NewRegionalReorgPolicy("code-478", "name-516");
        try
        {
            item.MoveTo(RegionalReorgPolicyStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalReorgPolicyStatus.Queued, "status");
    }

    public static void RegionalReorgPolicyRejectsMovesFromFailed()
    {
        var item = RegionalReorgPolicy.NewRegionalReorgPolicy("code-478", "name-516");
        item.Status = RegionalReorgPolicyStatus.Failed;
        Check.Fails(() => item.MoveTo(RegionalReorgPolicyStatus.New), "move");
    }

    public static void RunAll()
    {
        RegionalReorgPolicyStartsInNew();
        RegionalReorgPolicyMovesToQueued();
        RegionalReorgPolicyRejectsMovesFromFailed();
    }
}
