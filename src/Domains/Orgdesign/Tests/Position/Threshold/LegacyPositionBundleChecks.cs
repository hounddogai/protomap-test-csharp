namespace Acme.Workforce.Orgdesign.Tests.Position.Threshold;

using Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LegacyPositionBundleChecks
{
    public static void LegacyPositionBundleStartsInNew()
    {
        var item = LegacyPositionBundle.NewLegacyPositionBundle("code-673", "label-832");
        Check.Equal(item.Status, LegacyPositionBundleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPositionBundleMovesToDone()
    {
        var item = LegacyPositionBundle.NewLegacyPositionBundle("code-673", "label-832");
        try
        {
            item.MoveTo(LegacyPositionBundleStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPositionBundleStatus.Done, "status");
    }

    public static void LegacyPositionBundleRejectsMovesFromFailed()
    {
        var item = LegacyPositionBundle.NewLegacyPositionBundle("code-673", "label-832");
        item.Status = LegacyPositionBundleStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacyPositionBundleStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacyPositionBundleStartsInNew();
        LegacyPositionBundleMovesToDone();
        LegacyPositionBundleRejectsMovesFromFailed();
    }
}
