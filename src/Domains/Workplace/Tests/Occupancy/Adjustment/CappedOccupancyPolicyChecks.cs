namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedOccupancyPolicyChecks
{
    public static void CappedOccupancyPolicyStartsInOpen()
    {
        var item = CappedOccupancyPolicy.NewCappedOccupancyPolicy("code-696", "name-924");
        Check.Equal(item.Status, CappedOccupancyPolicyStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedOccupancyPolicyMovesToInReview()
    {
        var item = CappedOccupancyPolicy.NewCappedOccupancyPolicy("code-696", "name-924");
        try
        {
            item.MoveTo(CappedOccupancyPolicyStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedOccupancyPolicyStatus.InReview, "status");
    }

    public static void CappedOccupancyPolicyRejectsMovesFromArchived()
    {
        var item = CappedOccupancyPolicy.NewCappedOccupancyPolicy("code-696", "name-924");
        item.Status = CappedOccupancyPolicyStatus.Archived;
        Check.Fails(() => item.MoveTo(CappedOccupancyPolicyStatus.Open), "move");
    }

    public static void RunAll()
    {
        CappedOccupancyPolicyStartsInOpen();
        CappedOccupancyPolicyMovesToInReview();
        CappedOccupancyPolicyRejectsMovesFromArchived();
    }
}
