namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class MobilityWindowChecks
{
    public static void MobilityWindowStartsInNew()
    {
        var item = MobilityWindow.NewMobilityWindow("code-781", "label-141");
        Check.Equal(item.Status, MobilityWindowStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityWindowMovesToQueued()
    {
        var item = MobilityWindow.NewMobilityWindow("code-781", "label-141");
        try
        {
            item.MoveTo(MobilityWindowStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityWindowStatus.Queued, "status");
    }

    public static void MobilityWindowRejectsMovesFromFailed()
    {
        var item = MobilityWindow.NewMobilityWindow("code-781", "label-141");
        item.Status = MobilityWindowStatus.Failed;
        Check.Fails(() => item.MoveTo(MobilityWindowStatus.New), "move");
    }

    public static void RunAll()
    {
        MobilityWindowStartsInNew();
        MobilityWindowMovesToQueued();
        MobilityWindowRejectsMovesFromFailed();
    }
}
