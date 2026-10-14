namespace Acme.Workforce.Workplace.Tests.AccessBadge.Threshold;

using Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class WeightedAccessBadgeExceptionChecks
{
    public static void WeightedAccessBadgeExceptionStartsInProposed()
    {
        var item = WeightedAccessBadgeException.NewWeightedAccessBadgeException("code-345", "label-357");
        Check.Equal(item.Status, WeightedAccessBadgeExceptionStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAccessBadgeExceptionMovesToWithdrawn()
    {
        var item = WeightedAccessBadgeException.NewWeightedAccessBadgeException("code-345", "label-357");
        try
        {
            item.MoveTo(WeightedAccessBadgeExceptionStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAccessBadgeExceptionStatus.Withdrawn, "status");
    }

    public static void WeightedAccessBadgeExceptionRejectsMovesFromWithdrawn()
    {
        var item = WeightedAccessBadgeException.NewWeightedAccessBadgeException("code-345", "label-357");
        item.Status = WeightedAccessBadgeExceptionStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedAccessBadgeExceptionStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeExceptionStartsInProposed();
        WeightedAccessBadgeExceptionMovesToWithdrawn();
        WeightedAccessBadgeExceptionRejectsMovesFromWithdrawn();
    }
}
