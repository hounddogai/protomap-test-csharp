namespace Acme.Workforce.Talent.Tests.Mobility.MobilityRecord;

using Acme.Workforce.Talent.Mobility.MobilityRecord;

/// <summary>Checks of mobility record code, run by the project's runner.</summary>
public static class MobilityAssignmentChecks
{
    public static void MobilityAssignmentStartsInCreated()
    {
        var item = MobilityAssignment.NewMobilityAssignment("code-254", "label-78");
        Check.Equal(item.Status, MobilityAssignmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityAssignmentMovesToPublished()
    {
        var item = MobilityAssignment.NewMobilityAssignment("code-254", "label-78");
        try
        {
            item.MoveTo(MobilityAssignmentStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityAssignmentStatus.Published, "status");
    }

    public static void MobilityAssignmentRejectsMovesFromRetired()
    {
        var item = MobilityAssignment.NewMobilityAssignment("code-254", "label-78");
        item.Status = MobilityAssignmentStatus.Retired;
        Check.Fails(() => item.MoveTo(MobilityAssignmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        MobilityAssignmentStartsInCreated();
        MobilityAssignmentMovesToPublished();
        MobilityAssignmentRejectsMovesFromRetired();
    }
}
