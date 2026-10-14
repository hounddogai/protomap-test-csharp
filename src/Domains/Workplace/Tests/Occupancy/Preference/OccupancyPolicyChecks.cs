namespace Acme.Workforce.Workplace.Tests.Occupancy.Preference;

using Acme.Workforce.Workplace.Occupancy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class OccupancyPolicyChecks
{
    public static void OccupancyPolicyStartsInOpen()
    {
        var item = OccupancyPolicy.NewOccupancyPolicy("code-296", "title-578");
        Check.Equal(item.Status, OccupancyPolicyStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyPolicyMovesToResolved()
    {
        var item = OccupancyPolicy.NewOccupancyPolicy("code-296", "title-578");
        try
        {
            item.MoveTo(OccupancyPolicyStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyPolicyStatus.Resolved, "status");
    }

    public static void OccupancyPolicyRejectsMovesFromArchived()
    {
        var item = OccupancyPolicy.NewOccupancyPolicy("code-296", "title-578");
        item.Status = OccupancyPolicyStatus.Archived;
        Check.Fails(() => item.MoveTo(OccupancyPolicyStatus.Open), "move");
    }

    public static void RunAll()
    {
        OccupancyPolicyStartsInOpen();
        OccupancyPolicyMovesToResolved();
        OccupancyPolicyRejectsMovesFromArchived();
    }
}
