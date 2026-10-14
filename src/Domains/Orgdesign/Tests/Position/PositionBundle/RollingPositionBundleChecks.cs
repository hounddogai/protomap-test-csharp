namespace Acme.Workforce.Orgdesign.Tests.Position.PositionBundle;

using Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>Checks of position bundle code, run by the project's runner.</summary>
public static class RollingPositionBundleChecks
{
    public static void RollingPositionBundleStartsInNew()
    {
        var item = RollingPositionBundle.NewRollingPositionBundle("code-370", "label-635");
        Check.Equal(item.Status, RollingPositionBundleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPositionBundleMovesToQueued()
    {
        var item = RollingPositionBundle.NewRollingPositionBundle("code-370", "label-635");
        try
        {
            item.MoveTo(RollingPositionBundleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPositionBundleStatus.Queued, "status");
    }

    public static void RollingPositionBundleRejectsMovesFromFailed()
    {
        var item = RollingPositionBundle.NewRollingPositionBundle("code-370", "label-635");
        item.Status = RollingPositionBundleStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingPositionBundleStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingPositionBundleStartsInNew();
        RollingPositionBundleMovesToQueued();
        RollingPositionBundleRejectsMovesFromFailed();
    }
}
