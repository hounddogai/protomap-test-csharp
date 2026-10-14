namespace Acme.Workforce.Talent.Tests.Mobility.Reservation;

using Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MobilityTemplateChecks
{
    public static void MobilityTemplateStartsInProposed()
    {
        var item = MobilityTemplate.NewMobilityTemplate("code-493", "label-195");
        Check.Equal(item.Status, MobilityTemplateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityTemplateMovesToApproved()
    {
        var item = MobilityTemplate.NewMobilityTemplate("code-493", "label-195");
        try
        {
            item.MoveTo(MobilityTemplateStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityTemplateStatus.Approved, "status");
    }

    public static void MobilityTemplateRejectsMovesFromWithdrawn()
    {
        var item = MobilityTemplate.NewMobilityTemplate("code-493", "label-195");
        item.Status = MobilityTemplateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(MobilityTemplateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        MobilityTemplateStartsInProposed();
        MobilityTemplateMovesToApproved();
        MobilityTemplateRejectsMovesFromWithdrawn();
    }
}
