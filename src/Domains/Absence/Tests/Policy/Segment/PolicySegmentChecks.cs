namespace Acme.Workforce.Absence.Tests.Policy.Segment;

using Acme.Workforce.Absence.Policy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PolicySegmentChecks
{
    public static void PolicySegmentStartsInCreated()
    {
        var item = PolicySegment.NewPolicySegment("code-858", "name-967");
        Check.Equal(item.Status, PolicySegmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicySegmentMovesToRetired()
    {
        var item = PolicySegment.NewPolicySegment("code-858", "name-967");
        try
        {
            item.MoveTo(PolicySegmentStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicySegmentStatus.Retired, "status");
    }

    public static void PolicySegmentRejectsMovesFromRetired()
    {
        var item = PolicySegment.NewPolicySegment("code-858", "name-967");
        item.Status = PolicySegmentStatus.Retired;
        Check.Fails(() => item.MoveTo(PolicySegmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        PolicySegmentStartsInCreated();
        PolicySegmentMovesToRetired();
        PolicySegmentRejectsMovesFromRetired();
    }
}
