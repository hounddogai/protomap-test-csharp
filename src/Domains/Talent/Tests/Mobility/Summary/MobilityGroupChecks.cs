namespace Acme.Workforce.Talent.Tests.Mobility.Summary;

using Acme.Workforce.Talent.Mobility.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class MobilityGroupChecks
{
    public static void MobilityGroupStartsInPlanned()
    {
        var item = MobilityGroup.NewMobilityGroup("code-128", "name-942");
        Check.Equal(item.Status, MobilityGroupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityGroupMovesToCancelled()
    {
        var item = MobilityGroup.NewMobilityGroup("code-128", "name-942");
        try
        {
            item.MoveTo(MobilityGroupStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityGroupStatus.Cancelled, "status");
    }

    public static void MobilityGroupRejectsMovesFromCancelled()
    {
        var item = MobilityGroup.NewMobilityGroup("code-128", "name-942");
        item.Status = MobilityGroupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(MobilityGroupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        MobilityGroupStartsInPlanned();
        MobilityGroupMovesToCancelled();
        MobilityGroupRejectsMovesFromCancelled();
    }
}
