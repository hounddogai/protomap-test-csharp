namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class RollingReorgBundleChecks
{
    public static void RollingReorgBundleStartsInNew()
    {
        var item = RollingReorgBundle.NewRollingReorgBundle("code-48", "label-545");
        Check.Equal(item.Status, RollingReorgBundleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingReorgBundleMovesToQueued()
    {
        var item = RollingReorgBundle.NewRollingReorgBundle("code-48", "label-545");
        try
        {
            item.MoveTo(RollingReorgBundleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingReorgBundleStatus.Queued, "status");
    }

    public static void RollingReorgBundleRejectsMovesFromFailed()
    {
        var item = RollingReorgBundle.NewRollingReorgBundle("code-48", "label-545");
        item.Status = RollingReorgBundleStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingReorgBundleStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingReorgBundleStartsInNew();
        RollingReorgBundleMovesToQueued();
        RollingReorgBundleRejectsMovesFromFailed();
    }
}
