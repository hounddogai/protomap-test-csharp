namespace Acme.Workforce.Assets.Tests.Assignment.Summary;

using Acme.Workforce.Assets.Assignment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssignmentSummaryChecks
{
    public static void AssignmentSummaryStartsInCreated()
    {
        var item = AssignmentSummary.NewAssignmentSummary("code-279", "name-198");
        Check.Equal(item.Status, AssignmentSummaryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentSummaryMovesToValidated()
    {
        var item = AssignmentSummary.NewAssignmentSummary("code-279", "name-198");
        try
        {
            item.MoveTo(AssignmentSummaryStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentSummaryStatus.Validated, "status");
    }

    public static void AssignmentSummaryRejectsMovesFromRetired()
    {
        var item = AssignmentSummary.NewAssignmentSummary("code-279", "name-198");
        item.Status = AssignmentSummaryStatus.Retired;
        Check.Fails(() => item.MoveTo(AssignmentSummaryStatus.Created), "move");
    }

    public static void RunAll()
    {
        AssignmentSummaryStartsInCreated();
        AssignmentSummaryMovesToValidated();
        AssignmentSummaryRejectsMovesFromRetired();
    }
}
